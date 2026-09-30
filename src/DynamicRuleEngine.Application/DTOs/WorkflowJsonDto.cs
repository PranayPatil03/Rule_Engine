namespace DynamicRuleEngine.Application.DTOs;
public sealed class WorkflowJsonDto
{
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public int Version { get; set; }
    public string SourceHash { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
}
