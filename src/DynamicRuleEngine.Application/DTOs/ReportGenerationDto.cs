namespace DynamicRuleEngine.Application.DTOs;

public sealed class GenerateReportResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public int WorkflowJsonVersion { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public List<ValidationErrorDto> ValidationErrors { get; set; } = [];
    public List<CalculationResultDto> Calculations { get; set; } = [];
    public Dictionary<string, object?> Inputs { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
