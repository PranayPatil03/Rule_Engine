using DynamicRuleEngine.Domain.Enums;
namespace DynamicRuleEngine.Domain.Entities;
public sealed class RuleDefinition
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public RuleType RuleType { get; set; }
    public string ConditionExpression { get; set; } = string.Empty;
    public string? CalculationExpression { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessEvent { get; set; }
    public bool Enabled { get; set; } = true;
    public int SortOrder { get; set; }
    public WorkflowDefinition Workflow { get; set; } = null!;
}
