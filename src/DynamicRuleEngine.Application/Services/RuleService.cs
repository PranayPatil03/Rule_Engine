using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Domain.Enums;

namespace DynamicRuleEngine.Application.Services;

public sealed class RuleService : IRuleService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IRuleRepository _ruleRepository;

    public RuleService(
        IWorkflowRepository workflowRepository,
        IRuleRepository ruleRepository)
    {
        _workflowRepository = workflowRepository;
        _ruleRepository = ruleRepository;
    }

    public async Task<IReadOnlyList<RuleDto>> GetAsync(
        string workflowCode,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var items = await _ruleRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<RuleDto> CreateAsync(
        string workflowCode,
        RuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        Validate(dto);

        var items = await _ruleRepository.GetByWorkflowIdAsync(workflow.Id, cancellationToken);
        if (items.Any(x => x.RuleCode.Equals(dto.RuleCode, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Rule code already exists.");

        var entity = new RuleDefinition
        {
            WorkflowId = workflow.Id,
            RuleCode = dto.RuleCode.Trim(),
            RuleName = dto.RuleName.Trim(),
            RuleType = dto.RuleType,
            ConditionExpression = dto.ConditionExpression.Trim(),
            CalculationExpression = EmptyToNull(dto.CalculationExpression),
            ErrorMessage = EmptyToNull(dto.ErrorMessage),
            SuccessEvent = EmptyToNull(dto.SuccessEvent),
            Enabled = dto.Enabled,
            SortOrder = dto.SortOrder
        };

        await _ruleRepository.AddAsync(entity, cancellationToken);
        await _ruleRepository.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<RuleDto> UpdateAsync(
        string workflowCode,
        RuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        Validate(dto);

        var entity = await _ruleRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException("Rule not found.");

        if (entity.WorkflowId != workflow.Id)
            throw new InvalidOperationException("Rule belongs to another workflow.");

        if (!entity.RuleCode.Equals(dto.RuleCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("RuleCode cannot be changed.");

        entity.RuleName = dto.RuleName.Trim();
        entity.RuleType = dto.RuleType;
        entity.ConditionExpression = dto.ConditionExpression.Trim();
        entity.CalculationExpression = EmptyToNull(dto.CalculationExpression);
        entity.ErrorMessage = EmptyToNull(dto.ErrorMessage);
        entity.SuccessEvent = EmptyToNull(dto.SuccessEvent);
        entity.Enabled = dto.Enabled;
        entity.SortOrder = dto.SortOrder;

        _ruleRepository.Update(entity);
        await _ruleRepository.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task DeleteAsync(
        string workflowCode,
        int id,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetWorkflow(workflowCode, cancellationToken);
        var entity = await _ruleRepository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException("Rule not found.");

        if (entity.WorkflowId != workflow.Id)
            throw new InvalidOperationException("Rule belongs to another workflow.");

        _ruleRepository.Remove(entity);
        await _ruleRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<WorkflowDefinition> GetWorkflow(
        string workflowCode,
        CancellationToken cancellationToken)
    {
        var workflow = await _workflowRepository.GetByCodeAsync(workflowCode, cancellationToken);
        if (workflow is null)
            throw new KeyNotFoundException($"Workflow '{workflowCode}' was not found.");

        return workflow;
    }

    private static void Validate(RuleDto dto)
    {
        if (dto.Id < 0)
            throw new ArgumentException("Invalid rule Id.");

        if (string.IsNullOrWhiteSpace(dto.RuleCode))
            throw new ArgumentException("RuleCode is required.");

        if (string.IsNullOrWhiteSpace(dto.RuleName))
            throw new ArgumentException("RuleName is required.");

        if (string.IsNullOrWhiteSpace(dto.ConditionExpression))
            throw new ArgumentException("ConditionExpression is required.");

        if (dto.RuleType == RuleType.Calculation && string.IsNullOrWhiteSpace(dto.CalculationExpression))
            throw new ArgumentException("CalculationExpression is required for a calculation rule.");
    }

    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static RuleDto ToDto(RuleDefinition x)
    {
        return new RuleDto
        {
            Id = x.Id,
            RuleCode = x.RuleCode,
            RuleName = x.RuleName,
            RuleType = x.RuleType,
            ConditionExpression = x.ConditionExpression,
            CalculationExpression = x.CalculationExpression,
            ErrorMessage = x.ErrorMessage,
            SuccessEvent = x.SuccessEvent,
            Enabled = x.Enabled,
            SortOrder = x.SortOrder
        };
    }
}
