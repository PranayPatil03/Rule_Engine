using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;

namespace DynamicRuleEngine.Application.Services;

public sealed class ParameterService : IParameterService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IParameterRepository _parameterRepository;

    public ParameterService(IWorkflowRepository workflowRepository, IParameterRepository parameterRepository)
    {
        _workflowRepository = workflowRepository;
        _parameterRepository = parameterRepository;
    }

    public async Task<IReadOnlyList<ParameterDto>> GetAsync(string workflowCode, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var items = await _parameterRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<ParameterDto> CreateAsync(string workflowCode, ParameterDto dto, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        Validate(dto);

        var items = await _parameterRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);
        if (items.Any(x => x.ParameterCode.Equals(dto.ParameterCode, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Parameter code already exists.");

        var entity = new ParameterDefinition
        {
            WorkflowId = workflow.Id,
            ParameterCode = dto.ParameterCode.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            DataType = dto.DataType,
            IsRequired = dto.IsRequired,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            Weight = dto.Weight,
            WhyItMatters = dto.WhyItMatters,
            HowToMeasure = dto.HowToMeasure
        };

        foreach (var score in dto.ScoreDefinitions.Where(x => x.Score is >= 1 and <= 5))
        {
            entity.ScoreDefinitions.Add(new ScoreDefinition
            {
                Score = score.Score,
                Description = score.Description?.Trim() ?? string.Empty
            });
        }

        await _parameterRepository.AddAsync(entity, cancellationToken);
        await _parameterRepository.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ParameterDto> UpdateAsync(string workflowCode, ParameterDto dto, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        Validate(dto);

        var entity = await _parameterRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException("Parameter not found.");
        if (entity.WorkflowId != workflow.Id)
            throw new InvalidOperationException("Parameter belongs to another workflow.");
        if (!entity.ParameterCode.Equals(dto.ParameterCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("ParameterCode cannot be changed.");

        entity.DisplayName = dto.DisplayName.Trim();
        entity.DataType = dto.DataType;
        entity.IsRequired = dto.IsRequired;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;
        entity.Weight = dto.Weight;
        entity.WhyItMatters = dto.WhyItMatters;
        entity.HowToMeasure = dto.HowToMeasure;

        entity.ScoreDefinitions.Clear();
        foreach (var score in dto.ScoreDefinitions.Where(x => x.Score is >= 1 and <= 5))
        {
            entity.ScoreDefinitions.Add(new ScoreDefinition
            {
                ParameterDefinitionId = entity.Id,
                Score = score.Score,
                Description = score.Description?.Trim() ?? string.Empty
            });
        }

        _parameterRepository.Update(entity);
        await _parameterRepository.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(string workflowCode, int id, CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var entity = await _parameterRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException("Parameter not found.");
        if (entity.WorkflowId != workflow.Id)
            throw new InvalidOperationException("Parameter belongs to another workflow.");

        _parameterRepository.Remove(entity);
        await _parameterRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<WorkflowDefinition> GetWorkflow(string workflowCode, CancellationToken cancellationToken)
    {
        var workflow = await _workflowRepository.GetByCodeAsync(workflowCode, cancellationToken);
        if (workflow is null)
            throw new KeyNotFoundException($"Workflow '{workflowCode}' was not found.");
        return workflow;
    }

    private static void Validate(ParameterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ParameterCode)) throw new ArgumentException("ParameterCode is required.");
        if (string.IsNullOrWhiteSpace(dto.DisplayName)) throw new ArgumentException("DisplayName is required.");
        if (dto.Weight < 0) throw new ArgumentException("Weight cannot be negative.");
    }

    public static ParameterDto ToDto(ParameterDefinition x) => new()
    {
        Id = x.Id,
        ParameterCode = x.ParameterCode,
        DisplayName = x.DisplayName,
        DataType = x.DataType,
        IsRequired = x.IsRequired,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        Weight = x.Weight,
        WhyItMatters = x.WhyItMatters,
        HowToMeasure = x.HowToMeasure,
        ScoreDefinitions = x.ScoreDefinitions.OrderBy(s => s.Score).Select(s => new ScoreDefinitionDto
        {
            Id = s.Id,
            Score = s.Score,
            Description = s.Description
        }).ToList()
    };
}
