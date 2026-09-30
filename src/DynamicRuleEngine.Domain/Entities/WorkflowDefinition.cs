namespace DynamicRuleEngine.Domain.Entities;
public sealed class WorkflowDefinition
{
    public int Id { get; set; }
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
    public ICollection<RuleDefinition> Rules { get; set; } = new List<RuleDefinition>();
    public WorkflowJson? WorkflowJson { get; set; }
}
