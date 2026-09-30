using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Domain.Enums;
using DynamicRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;

namespace DynamicRuleEngine.Infrastructure.Rules;

public sealed class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IWorkflowJsonRepository _jsonRepository;
    private readonly IRuleEngineService _ruleEngineService;
    private readonly AppDbContext _db;

    public WorkflowService(
        IWorkflowRepository workflowRepository,
        IWorkflowJsonRepository jsonRepository,
        IRuleEngineService ruleEngineService,
        AppDbContext db)
    {
        _workflowRepository = workflowRepository;
        _jsonRepository = jsonRepository;
        _ruleEngineService = ruleEngineService;
        _db = db;
    }

    public async Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await _workflowRepository.GetAllAsync(cancellationToken);
        return workflows.Select(x => new WorkflowSummaryDto
        {
            Id = x.Id,
            WorkflowCode = x.WorkflowCode,
            WorkflowName = x.WorkflowName
        }).ToList();
    }

    public async Task<WorkflowJsonDto> GetCurrentJsonAsync(string workflowCode, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var json = await _jsonRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);
        if (json is null)
            throw new InvalidOperationException($"Workflow '{workflowCode}' has not been published yet.");
        return ToDto(workflow, json);
    }

    public async Task<WorkflowJsonDto> PublishAsync(string workflowCode, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var parameters = await _db.Parameters.Include(x => x.ScoreDefinitions)
            .Where(x => x.WorkflowId == workflow.Id).OrderBy(x => x.SortOrder).AsNoTracking().ToListAsync(cancellationToken);
        var rules = await _db.Rules.Where(x => x.WorkflowId == workflow.Id)
            .OrderBy(x => x.SortOrder).AsNoTracking().ToListAsync(cancellationToken);

        if (rules.Count == 0)
            throw new InvalidOperationException("Add at least one rule before publishing.");

        var json = BuildJson(workflow, parameters, rules);
        return await SaveJsonInternalAsync(workflow, json, cancellationToken);
    }

    public async Task<WorkflowJsonDto> SaveJsonAsync(string workflowCode, string jsonContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            throw new ArgumentException("Workflow JSON cannot be empty.");

        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        ValidateRulesEngineJson(jsonContent);

        var document = DeserializeDocument(jsonContent);
        if (!document.WorkflowName.Equals(workflow.WorkflowCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"JSON workflowName must be '{workflow.WorkflowCode}'.");

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var existingParameters = await _db.Parameters
            .Include(x => x.ScoreDefinitions)
            .Where(x => x.WorkflowId == workflow.Id)
            .ToListAsync(cancellationToken);

        var existingRules = await _db.Rules
            .Where(x => x.WorkflowId == workflow.Id)
            .ToListAsync(cancellationToken);

        _db.Parameters.RemoveRange(existingParameters);
        _db.Rules.RemoveRange(existingRules);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var item in document.Parameters.OrderBy(x => x.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(item.ParameterCode))
                throw new InvalidOperationException("Every JSON parameter needs parameterCode.");

            var parameter = new ParameterDefinition
            {
                WorkflowId = workflow.Id,
                ParameterCode = item.ParameterCode.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ParameterCode.Trim() : item.DisplayName.Trim(),
                DataType = ParseDataType(item.DataType),
                IsRequired = item.IsRequired,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
                Weight = item.Weight,
                WhyItMatters = item.WhyItMatters,
                HowToMeasure = item.HowToMeasure
            };

            foreach (var score in item.ScoreDefinitions.Where(x => x.Score is >= 1 and <= 5))
            {
                parameter.ScoreDefinitions.Add(new ScoreDefinition
                {
                    Score = score.Score,
                    Description = score.Description ?? string.Empty
                });
            }

            _db.Parameters.Add(parameter);
        }

        foreach (var item in document.Rules.OrderBy(x => x.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(item.RuleCode) || string.IsNullOrWhiteSpace(item.Expression))
                throw new InvalidOperationException("Every JSON rule needs ruleName and expression.");

            var ruleType = item.RuleType.Equals("Calculation", StringComparison.OrdinalIgnoreCase)
                ? RuleType.Calculation
                : RuleType.Validation;

            _db.Rules.Add(new RuleDefinition
            {
                WorkflowId = workflow.Id,
                RuleCode = item.RuleCode.Trim(),
                RuleName = string.IsNullOrWhiteSpace(item.RuleDisplayName) ? item.RuleCode.Trim() : item.RuleDisplayName.Trim(),
                RuleType = ruleType,
                ConditionExpression = item.Expression,
                CalculationExpression = item.CalculationExpression,
                ErrorMessage = item.ErrorMessage,
                SuccessEvent = item.SuccessEvent,
                Enabled = item.Enabled,
                SortOrder = item.SortOrder
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var result = await SaveJsonInternalAsync(workflow, jsonContent, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<WorkflowJsonDto> SaveJsonInternalAsync(WorkflowDefinition workflow, string json, CancellationToken cancellationToken)
    {
        var normalized = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<JsonElement>(json),
            new JsonSerializerOptions { WriteIndented = true });

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
        var existing = await _jsonRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);

        if (existing is not null && existing.SourceHash == hash)
            return ToDto(workflow, existing);

        var entity = new WorkflowJson
        {
            Id = existing?.Id ?? 0,
            WorkflowId = workflow.Id,
            Version = existing?.Version + 1 ?? 1,
            SourceHash = hash,
            JsonContent = normalized,
            GeneratedAtUtc = DateTime.UtcNow
        };

        await _jsonRepository.UpsertAsync(entity, cancellationToken);
        _ruleEngineService.Invalidate(workflow.WorkflowCode, entity.Version);
        return ToDto(workflow, entity);
    }

    private async Task<WorkflowDefinition> GetWorkflow(string workflowCode, CancellationToken cancellationToken)
    {
        var workflow = await _workflowRepository.GetByCodeAsync(workflowCode, cancellationToken);
        if (workflow is null)
            throw new KeyNotFoundException($"Workflow '{workflowCode}' was not found.");
        return workflow;
    }

    private static string BuildJson(
        WorkflowDefinition workflow,
        IReadOnlyList<ParameterDefinition> parameters,
        IReadOnlyList<RuleDefinition> rules)
    {
        // Publish only serializes the rule definitions that are already configured.
        // It MUST NOT create, modify, or calculate any business formula.
        // CalculationExpression comes from the configured rule and is persisted
        // into WorkflowJson. At runtime Microsoft RulesEngine executes the
        // published JSON.
        var jsonRules = rules
            .Where(x => x.Enabled)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                ruleName = x.RuleCode,
                ruleDisplayName = x.RuleName,
                ruleType = x.RuleType.ToString(),
                errorMessage = x.ErrorMessage,
                enabled = x.Enabled,
                ruleExpressionType = "LambdaExpression",
                expression = x.ConditionExpression,
                successEvent = x.SuccessEvent,
                sortOrder = x.SortOrder,
                actions = x.RuleType == RuleType.Calculation
                    ? new
                    {
                        onSuccess = new
                        {
                            name = "OutputExpression",
                            context = new Dictionary<string, object?>
                            {
                                ["expression"] = x.CalculationExpression
                            }
                        }
                    }
                    : null
            })
            .Cast<object>()
            .ToList();

        var document = new
        {
            workflowName = workflow.WorkflowCode,
            workflowDisplayName = workflow.WorkflowName,
            ruleExpressionType = "LambdaExpression",
            parameters = parameters.Select(x => new
            {
                parameterCode = x.ParameterCode,
                displayName = x.DisplayName,
                dataType = x.DataType.ToString(),
                isRequired = x.IsRequired,
                sortOrder = x.SortOrder,
                isActive = x.IsActive,
                weight = x.Weight,
                whyItMatters = x.WhyItMatters,
                howToMeasure = x.HowToMeasure,
                scoreDefinitions = x.ScoreDefinitions
                    .OrderBy(s => s.Score)
                    .Select(s => new
                    {
                        score = s.Score,
                        description = s.Description
                    })
                    .ToList()
            }).ToList(),
            rules = jsonRules
        };

        return JsonSerializer.Serialize(
            new[] { document },
            new JsonSerializerOptions { WriteIndented = true });
    }

    private static void ValidateRulesEngineJson(string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        try
        {
            var workflows = JsonSerializer.Deserialize<Workflow[]>(json, options);
            if (workflows is null || workflows.Length == 0)
                throw new InvalidOperationException("JSON must contain at least one RulesEngine workflow.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("The supplied JSON is not a valid RulesEngine workflow JSON.", ex);
        }
    }

    private static WorkflowDocument DeserializeDocument(string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        var docs = JsonSerializer.Deserialize<List<WorkflowDocument>>(json, options);
        if (docs is null || docs.Count == 0)
            throw new InvalidOperationException("JSON must contain one workflow object.");

        return docs[0];
    }

    private static ParameterDataType ParseDataType(string? value)
        => Enum.TryParse<ParameterDataType>(value, true, out var result)
            ? result
            : throw new InvalidOperationException($"Unsupported parameter dataType '{value}'.");

    private static WorkflowJsonDto ToDto(WorkflowDefinition workflow, WorkflowJson json) => new()
    {
        WorkflowCode = workflow.WorkflowCode,
        WorkflowName = workflow.WorkflowName,
        Version = json.Version,
        SourceHash = json.SourceHash,
        JsonContent = json.JsonContent,
        GeneratedAtUtc = json.GeneratedAtUtc
    };

    private sealed class WorkflowDocument
    {
        public string WorkflowName { get; set; } = string.Empty;
        public List<JsonParameter> Parameters { get; set; } = [];
        public List<JsonRule> Rules { get; set; } = [];
    }

    private sealed class JsonParameter
    {
        public string ParameterCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = "String";
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal Weight { get; set; }
        public string? WhyItMatters { get; set; }
        public string? HowToMeasure { get; set; }
        public List<JsonScore> ScoreDefinitions { get; set; } = [];
    }

    private sealed class JsonScore
    {
        public int Score { get; set; }
        public string? Description { get; set; }
    }

    private sealed class JsonRule
    {
        public string RuleName { get; set; } = string.Empty;
        public string? RuleDisplayName { get; set; }
        public string RuleType { get; set; } = "Validation";
        public string? ErrorMessage { get; set; }
        public bool Enabled { get; set; } = true;
        public string Expression { get; set; } = string.Empty;
        public string? SuccessEvent { get; set; }
        public JsonActions? Actions { get; set; }
        public int SortOrder { get; set; }
        public string RuleCode => RuleName;
        public string? CalculationExpression
        {
            get
            {
                if (Actions?.OnSuccess?.Context is null) return null;
                return Actions.OnSuccess.Context.TryGetValue("expression", out var value)
                    ? value.ToString()
                    : null;
            }
        }
    }

    private sealed class JsonActions
    {
        public JsonAction? OnSuccess { get; set; }
    }

    private sealed class JsonAction
    {
        public string? Name { get; set; }
        public Dictionary<string, JsonElement>? Context { get; set; }
    }
}
