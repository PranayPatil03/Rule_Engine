namespace DynamicRuleEngine.Application.DTOs;
public sealed class ValidationErrorDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
public sealed class CalculationResultDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public object? Value { get; set; }
}
public sealed class EvaluateResponseDto
{
    public bool IsValid { get; set; }
    public int WorkflowJsonVersion { get; set; }
    public List<ValidationErrorDto> ValidationErrors { get; set; } = [];
    public List<CalculationResultDto> Calculations { get; set; } = [];
}
