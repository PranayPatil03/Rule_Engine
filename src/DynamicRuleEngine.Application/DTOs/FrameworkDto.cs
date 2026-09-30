namespace DynamicRuleEngine.Application.DTOs;

public sealed class FrameworkDto
{
    public WorkflowSummaryDto Workflow { get; set; } = new();

    public List<ParameterDto> Parameters { get; set; } = [];
}

public sealed class FrameworkEvaluationRequestDto
{
    public string WorkflowCode { get; set; } = string.Empty;

    public Dictionary<string, int> Scores { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
