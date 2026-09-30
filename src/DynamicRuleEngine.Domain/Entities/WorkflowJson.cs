namespace DynamicRuleEngine.Domain.Entities;
public sealed class WorkflowJson
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public int Version { get; set; }
    public string SourceHash { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public WorkflowDefinition Workflow { get; set; } = null!;
}
