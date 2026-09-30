using DynamicRuleEngine.Domain.Entities;
namespace DynamicRuleEngine.Application.Interfaces;
public interface IWorkflowJsonRepository
{
    Task<WorkflowJson?> GetByWorkflowIdAsync(int workflowId, CancellationToken cancellationToken = default);
    Task UpsertAsync(WorkflowJson entity, CancellationToken cancellationToken = default);
}
