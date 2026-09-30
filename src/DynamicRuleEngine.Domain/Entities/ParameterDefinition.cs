using DynamicRuleEngine.Domain.Enums;

namespace DynamicRuleEngine.Domain.Entities;

public sealed class ParameterDefinition
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ParameterDataType DataType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Weight { get; set; }
    public string? WhyItMatters { get; set; }
    public string? HowToMeasure { get; set; }
    public WorkflowDefinition Workflow { get; set; } = null!;
    public ICollection<ScoreDefinition> ScoreDefinitions { get; set; } = new List<ScoreDefinition>();
}
