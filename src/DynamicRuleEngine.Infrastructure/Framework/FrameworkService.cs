using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicRuleEngine.Infrastructure.Framework;

public sealed class FrameworkService : IFrameworkService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IParameterRepository _parameterRepository;
    private readonly IWorkflowJsonRepository _workflowJsonRepository;

    public FrameworkService(
        IWorkflowRepository workflowRepository,
        IParameterRepository parameterRepository,
        IWorkflowJsonRepository workflowJsonRepository)
    {
        _workflowRepository = workflowRepository;
        _parameterRepository = parameterRepository;
        _workflowJsonRepository = workflowJsonRepository;
    }

    public async Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflowsAsync(
        CancellationToken cancellationToken = default)
    {
        var workflows =
            await _workflowRepository.GetAllAsync(cancellationToken);

        return workflows
            .Select(x => new WorkflowSummaryDto
            {
                Id = x.Id,
                WorkflowCode = x.WorkflowCode,
                WorkflowName = x.WorkflowName
            })
            .ToList();
    }

    public async Task<FrameworkDto> GetAsync(
        string workflowCode,
        CancellationToken cancellationToken = default)
    {
        var workflow =
            await _workflowRepository.GetByCodeAsync(
                workflowCode,
                cancellationToken);

        if (workflow is null)
        {
            throw new KeyNotFoundException(
                $"Workflow '{workflowCode}' was not found.");
        }

        // IMPORTANT:
        // Framework reads ONLY the published WorkflowJson.
        // It does not read ParameterDefinition directly.

        var workflowJson =
            await _workflowJsonRepository.GetByWorkflowIdAsync(
                workflow.Id,
                cancellationToken);

        if (workflowJson is null)
        {
            return new FrameworkDto
            {
                Workflow = new WorkflowSummaryDto
                {
                    Id = workflow.Id,
                    WorkflowCode = workflow.WorkflowCode,
                    WorkflowName = workflow.WorkflowName
                },

                Parameters = []
            };
        }

        var publishedWorkflows =
            JsonSerializer.Deserialize<List<PublishedWorkflowDto>>(
                workflowJson.JsonContent,

               new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true,
                   Converters =
    {
        new JsonStringEnumConverter()
    }
               });

        var publishedWorkflow =
            publishedWorkflows?.FirstOrDefault();

        if (publishedWorkflow is null)
        {
            throw new InvalidOperationException(
                $"Published WorkflowJson for '{workflowCode}' is invalid.");
        }

        return new FrameworkDto
        {
            Workflow = new WorkflowSummaryDto
            {
                Id = workflow.Id,
                WorkflowCode = workflow.WorkflowCode,
                WorkflowName = workflow.WorkflowName
            },

            Parameters = publishedWorkflow.Parameters
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new ParameterDto
                {
                    ParameterCode = x.ParameterCode,
                    DisplayName = x.DisplayName,
                    DataType = x.DataType,
                    IsRequired = x.IsRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Weight = x.Weight,
                    WhyItMatters = x.WhyItMatters,
                    HowToMeasure = x.HowToMeasure,

                    ScoreDefinitions = x.ScoreDefinitions
                        .OrderBy(s => s.Score)
                        .Select(s => new ScoreDefinitionDto
                        {
                            Score = s.Score,
                            Description = s.Description
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}