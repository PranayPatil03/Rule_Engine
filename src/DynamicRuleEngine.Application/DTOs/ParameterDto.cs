using DynamicRuleEngine.Domain.Enums;

namespace DynamicRuleEngine.Application.DTOs;

public sealed class ScoreDefinitionDto
{
    public int Id { get; set; }
    public int Score { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class ParameterDto
{
    public int Id { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ParameterDataType DataType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public decimal Weight { get; set; }
    public string? WhyItMatters { get; set; }
    public string? HowToMeasure { get; set; }
    public List<ScoreDefinitionDto> ScoreDefinitions { get; set; } = [];
}
