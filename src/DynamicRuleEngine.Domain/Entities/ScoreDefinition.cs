namespace DynamicRuleEngine.Domain.Entities;

public sealed class ScoreDefinition
{
    public int Id { get; set; }
    public int ParameterDefinitionId { get; set; }
    public int Score { get; set; }
    public string Description { get; set; } = string.Empty;
    public ParameterDefinition ParameterDefinition { get; set; } = null!;
}
