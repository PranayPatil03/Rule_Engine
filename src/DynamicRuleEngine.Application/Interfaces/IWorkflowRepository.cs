using DynamicRuleEngine.Domain.Entities;

namespace DynamicRuleEngine.Application.Interfaces;

public interface IWorkflowRepository
{
    Task<WorkflowDefinition?> GetByCodeAsync(string workflowCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
