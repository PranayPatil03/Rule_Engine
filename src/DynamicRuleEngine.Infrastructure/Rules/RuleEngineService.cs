using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Enums;

using Microsoft.Extensions.Logging;

using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace DynamicRuleEngine.Infrastructure.Rules;

public sealed class RuleEngineService : IRuleEngineService
{
    private sealed record EngineCacheItem(
        int Version,
        IRulesEngine Engine);

    private readonly IWorkflowRepository _workflowRepository;

    private readonly IParameterRepository _parameterRepository;

    private readonly IWorkflowJsonRepository _jsonRepository;

    private readonly IRuleRepository _ruleRepository;

    private readonly ILogger<RuleEngineService> _logger;

    private readonly ConcurrentDictionary<
        string,
        EngineCacheItem> _cache =
        new(StringComparer.OrdinalIgnoreCase);


    public RuleEngineService(
        IWorkflowRepository workflowRepository,
        IParameterRepository parameterRepository,
        IWorkflowJsonRepository jsonRepository,
        IRuleRepository ruleRepository,
        ILogger<RuleEngineService> logger)
    {
        _workflowRepository =
            workflowRepository;

        _parameterRepository =
            parameterRepository;

        _jsonRepository =
            jsonRepository;

        _ruleRepository =
            ruleRepository;

        _logger = logger;
    }


    public async Task<EvaluateResponseDto> EvaluateAsync(
        EvaluateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var workflow =
            await _workflowRepository.GetByCodeAsync(
                request.WorkflowCode,
                cancellationToken);

        if (workflow is null)
        {
            throw new KeyNotFoundException(
                $"Workflow '{request.WorkflowCode}' was not found.");
        }


        var json =
            await _jsonRepository.GetByWorkflowIdAsync(
                workflow.Id,
                cancellationToken);

        if (json is null)
        {
            throw new InvalidOperationException(
                $"Workflow '{request.WorkflowCode}' has not been published yet.");
        }


        var definitions =
            await _parameterRepository
                .GetByWorkflowIdAsync(
                    workflow.Id,
                    cancellationToken);


        var input =
            BuildInput(
                definitions,
                request.Parameters);


        var engine =
            GetEngine(
                workflow.WorkflowCode,
                json);


        var parameters =
            new[]
            {
                new RuleParameter(
                    "input1",
                    input)
            };


        var response =
            new EvaluateResponseDto
            {
                IsValid = true,
                WorkflowJsonVersion = json.Version
            };


        // ---------------------------------------------------------
        // VALIDATION
        // ---------------------------------------------------------

        var validationResults =
            await engine.ExecuteAllRulesAsync(
                workflow.WorkflowCode,
                parameters);


        foreach (var result in Flatten(
            validationResults))
        {
            if (result.Rule?.Enabled == false)
            {
                continue;
            }


            if (IsCalculationRule(result))
            {
                continue;
            }


            if (!result.IsSuccess)
            {
                response.IsValid = false;

                response.ValidationErrors.Add(
                    new ValidationErrorDto
                    {
                        RuleCode =
                            result.Rule?.RuleName
                            ?? "Unknown",

                        Message =
                            result.ExceptionMessage
                            ?? result.Rule?.ErrorMessage
                            ?? "Validation failed."
                    });
            }
        }


        // ---------------------------------------------------------
        // STOP IF VALIDATION FAILED
        // ---------------------------------------------------------

        if (!response.IsValid)
        {
            return response;
        }


        // ---------------------------------------------------------
        // CALCULATIONS
        // ---------------------------------------------------------

        var rules =
            await _ruleRepository
                .GetByWorkflowIdAsync(
                    workflow.Id,
                    cancellationToken);


        foreach (var rule in rules
            .Where(x =>
                x.RuleType == RuleType.Calculation
                && x.Enabled)
            .OrderBy(x => x.SortOrder))
        {
            try
            {
                var actionResult =
                    await engine.ExecuteActionWorkflowAsync(
                        workflow.WorkflowCode,
                        rule.RuleCode,
                        parameters);


                var result =
                    actionResult.Results?
                        .FirstOrDefault();


                if (result?.IsSuccess == true)
                {
                    response.Calculations.Add(
                        new CalculationResultDto
                        {
                            RuleCode =
                                rule.RuleCode,

                            RuleName =
                                rule.RuleName,

                            Value =
                                actionResult.Output
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Calculation rule {RuleCode} failed.",
                    rule.RuleCode);


                response.IsValid = false;


                response.ValidationErrors.Add(
                    new ValidationErrorDto
                    {
                        RuleCode =
                            rule.RuleCode,

                        Message =
                            ex.Message
                    });
            }
        }


        return response;
    }


    // =============================================================
    // FRAMEWORK CALCULATION
    // =============================================================

    public async Task<EvaluateResponseDto> EvaluateFrameworkAsync(
        FrameworkEvaluationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var workflow =
            await _workflowRepository.GetByCodeAsync(
                request.WorkflowCode,
                cancellationToken);

        if (workflow is null)
        {
            throw new KeyNotFoundException(
                $"Workflow '{request.WorkflowCode}' was not found.");
        }

        var json =
            await _jsonRepository.GetByWorkflowIdAsync(
                workflow.Id,
                cancellationToken);

        if (json is null)
        {
            throw new InvalidOperationException(
                $"Workflow '{request.WorkflowCode}' has not been published yet.");
        }

        var definitions =
            await _parameterRepository.GetByWorkflowIdAsync(
                workflow.Id,
                cancellationToken);

        var activeDefinitions = definitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToList();

        var response = new EvaluateResponseDto
        {
            IsValid = true,
            WorkflowJsonVersion = json.Version
        };

        if (activeDefinitions.Count == 0)
        {
            return response;
        }

        var allScoresProvided = activeDefinitions.All(
            x => request.Scores.ContainsKey(x.ParameterCode));

        var inputValues = new Dictionary<string, JsonElement>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var definition in activeDefinitions)
        {
            if (request.Scores.TryGetValue(
                    definition.ParameterCode,
                    out var score))
            {
                inputValues[$"Score_{definition.ParameterCode}"] =
                    JsonSerializer.SerializeToElement(score);
            }
        }

        inputValues["FrameworkHasScores"] =
            JsonSerializer.SerializeToElement(allScoresProvided);

        var input = new ExpandoObject();
        var inputDictionary = (IDictionary<string, object?>)input;

        foreach (var item in inputValues)
        {
            inputDictionary[item.Key] =
                ConvertJsonElementToObject(item.Value);
        }

        var engine = GetEngine(
            workflow.WorkflowCode,
            json);

        var parameters = new[]
        {
            new RuleParameter("input1", input)
        };

        var calculationRules = await _ruleRepository
            .GetByWorkflowIdAsync(
                workflow.Id,
                cancellationToken);

        foreach (var rule in calculationRules
            .Where(x => x.RuleType == RuleType.Calculation && x.Enabled)
            .OrderBy(x => x.SortOrder))
        {
            try
            {
                var actionResult =
                    await engine.ExecuteActionWorkflowAsync(
                        workflow.WorkflowCode,
                        rule.RuleCode,
                        parameters);

                var result = actionResult.Results?.FirstOrDefault();

                if (result?.IsSuccess == true)
                {
                    response.Calculations.Add(
                        new CalculationResultDto
                        {
                            RuleCode = rule.RuleCode,
                            RuleName = rule.RuleName,
                            Value = actionResult.Output
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Framework calculation rule {RuleCode} failed.",
                    rule.RuleCode);

                response.IsValid = false;
                response.ValidationErrors.Add(
                    new ValidationErrorDto
                    {
                        RuleCode = rule.RuleCode,
                        Message = ex.Message
                    });
            }
        }

        return response;
    }


    // =============================================================
    // INVALIDATE CACHE
    // =============================================================

    public void Invalidate(
        string workflowCode,
        int version)
    {
        if (_cache.TryGetValue(
                workflowCode,
                out var cached)
            && cached.Version != version)
        {
            _cache.TryRemove(
                workflowCode,
                out _);
        }
    }


    // =============================================================
    // GET ENGINE
    // =============================================================

    private IRulesEngine GetEngine(
        string workflowCode,
        DynamicRuleEngine.Domain.Entities.WorkflowJson json)
    {
        if (_cache.TryGetValue(
                workflowCode,
                out var cached)
            && cached.Version == json.Version)
        {
            return cached.Engine;
        }


        var options =
            new JsonSerializerOptions(
                JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };


        options.Converters.Add(
            new JsonStringEnumConverter());


        Workflow[]? workflows = null;


        try
        {
            workflows =
                JsonSerializer.Deserialize<Workflow[]>(
                    json.JsonContent,
                    options);
        }
        catch (JsonException)
        {
            // Support older JSON that contains
            // one Workflow object instead of an array.

            try
            {
                var singleWorkflow =
                    JsonSerializer.Deserialize<Workflow>(
                        json.JsonContent,
                        options);

                if (singleWorkflow is not null)
                {
                    workflows =
                        new[]
                        {
                            singleWorkflow
                        };
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "The stored WorkflowJson is invalid.",
                    ex);
            }
        }


        // IMPORTANT:
        // Do not use:
        //
        // workflows ??= throw new ...
        //
        // Use a normal if statement.

        if (workflows is null)
        {
            throw new InvalidOperationException(
                "Persisted WorkflowJson could not be deserialized. " +
                "Expected a RulesEngine workflow array.");
        }


        if (workflows.Length == 0)
        {
            throw new InvalidOperationException(
                "Persisted WorkflowJson contains no workflows.");
        }


        var engine =
            new RulesEngine.RulesEngine(
                workflows,
                new ReSettings
                {
                    AutoExecuteActions = false,

                    EnableExceptionAsErrorMessage = true,

                    EnableExceptionAsErrorMessageForRuleExpressionParsing =
                        true
                });


        _cache[workflowCode] =
            new EngineCacheItem(
                json.Version,
                engine);


        return engine;
    }


    // =============================================================
    // CALCULATION CHECK
    // =============================================================

    private static bool IsCalculationRule(
        RuleResultTree result)
    {
        return result.Rule?.Actions?.OnSuccess?.Name
            .Equals(
                "OutputExpression",
                StringComparison.OrdinalIgnoreCase)
            == true;
    }


    // =============================================================
    // FLATTEN RESULTS
    // =============================================================

    private static IEnumerable<RuleResultTree> Flatten(
        IEnumerable<RuleResultTree> source)
    {
        foreach (var item in source)
        {
            yield return item;


            if (item.ChildResults is not null)
            {
                foreach (var child in Flatten(
                    item.ChildResults))
                {
                    yield return child;
                }
            }
        }
    }


    // =============================================================
    // BUILD INPUT
    // =============================================================

    private static ExpandoObject BuildInput(
        IReadOnlyList<
            DynamicRuleEngine.Domain.Entities.ParameterDefinition>
            definitions,
        IReadOnlyDictionary<
            string,
            JsonElement>
            values)
    {
        var input =
            new ExpandoObject();


        var dictionary =
            (IDictionary<string, object?>)input;


        foreach (var definition in definitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder))
        {
            if (!values.TryGetValue(
                    definition.ParameterCode,
                    out var jsonValue)
                || jsonValue.ValueKind
                    is JsonValueKind.Null
                    or JsonValueKind.Undefined)
            {
                if (definition.IsRequired)
                {
                    throw new ArgumentException(
                        $"Required parameter '{definition.ParameterCode}' is missing.");
                }


                dictionary[
                    definition.ParameterCode] =
                    GetDefault(
                        definition.DataType);

                continue;
            }


            dictionary[
                definition.ParameterCode] =
                ConvertValue(
                    jsonValue,
                    definition.DataType,
                    definition.ParameterCode);
        }


        // Allow framework-only values (for example Score_<ParameterCode>)
        // to be supplied to calculation expressions stored in WorkflowJson.
        foreach (var extra in values)
        {
            if (dictionary.ContainsKey(extra.Key))
            {
                continue;
            }

            dictionary[extra.Key] =
                ConvertJsonElementToObject(extra.Value);
        }

        return input;
    }


    private static object? ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }


    // =============================================================
    // CONVERT VALUE
    // =============================================================

    private static object? ConvertValue(
        JsonElement element,
        ParameterDataType type,
        string parameterCode)
    {
        var raw =
            element.ValueKind ==
                JsonValueKind.String
                    ? element.GetString()
                    : element.GetRawText();


        if (string.IsNullOrWhiteSpace(raw))
        {
            if (type ==
                ParameterDataType.String)
            {
                return string.Empty;
            }


            throw new ArgumentException(
                $"Parameter '{parameterCode}' cannot be empty.");
        }


        try
        {
            return type switch
            {
                ParameterDataType.String =>
                    raw,

                ParameterDataType.Integer =>
                    int.Parse(
                        raw!,
                        CultureInfo.InvariantCulture),

                ParameterDataType.Decimal =>
                    decimal.Parse(
                        raw!,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture),

                ParameterDataType.Double =>
                    double.Parse(
                        raw!,
                        NumberStyles.Float |
                        NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture),

                ParameterDataType.Boolean =>
                    bool.Parse(raw!),

                ParameterDataType.Date =>
                    DateTime.Parse(
                        raw!,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal),

                _ =>
                    raw
            };
        }
        catch (Exception ex)
            when (ex is FormatException
                or OverflowException
                or ArgumentException)
        {
            throw new ArgumentException(
                $"Invalid value '{raw}' for parameter " +
                $"'{parameterCode}' of type {type}.",
                ex);
        }
    }


    // =============================================================
    // DEFAULT VALUE
    // =============================================================

    private static object? GetDefault(
        ParameterDataType type)
    {
        return type switch
        {
            ParameterDataType.String =>
                string.Empty,

            ParameterDataType.Integer =>
                0,

            ParameterDataType.Decimal =>
                0m,

            ParameterDataType.Double =>
                0d,

            ParameterDataType.Boolean =>
                false,

            ParameterDataType.Date =>
                DateTime.MinValue,

            _ =>
                null
        };
    }
}