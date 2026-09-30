namespace DynamicRuleEngine.Application.DTOs;

public sealed class WorkflowSummaryDto
{
    public int Id { get; set; }
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
}
