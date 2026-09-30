using DynamicRuleEngine.Application.DTOs;

namespace DynamicRuleEngine.Application.Interfaces;

public interface IFrameworkService
{
    Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflowsAsync(
        CancellationToken cancellationToken = default);

    Task<FrameworkDto> GetAsync(
        string workflowCode,
        CancellationToken cancellationToken = default);
}
