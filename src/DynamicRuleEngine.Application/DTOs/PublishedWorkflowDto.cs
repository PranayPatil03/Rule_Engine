using DynamicRuleEngine.Domain.Enums;

namespace DynamicRuleEngine.Application.DTOs;

public sealed class PublishedWorkflowDto
{
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public int Version { get; set; }

    public List<PublishedParameterDto> Parameters { get; set; } = [];
    public List<PublishedRuleDto> Rules { get; set; } = [];
}

public sealed class PublishedParameterDto
{
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

public sealed class PublishedRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleDisplayName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool Enabled { get; set; }
    public int SortOrder { get; set; }
}