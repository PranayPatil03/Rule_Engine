using DynamicRuleEngine.Application.DTOs;

namespace DynamicRuleEngine.Application.Interfaces;

public interface IWorkflowService
{
    Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default);
    Task<WorkflowJsonDto> GetCurrentJsonAsync(string workflowCode, CancellationToken cancellationToken = default);
    Task<WorkflowJsonDto> PublishAsync(string workflowCode, CancellationToken cancellationToken = default);
    Task<WorkflowJsonDto> SaveJsonAsync(string workflowCode, string jsonContent, CancellationToken cancellationToken = default);
}
