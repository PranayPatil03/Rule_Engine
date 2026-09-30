using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;

namespace DynamicRuleEngine.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
    private readonly IRuleEngineService _ruleEngineService;

    public ReportService(IRuleEngineService ruleEngineService)
    {
        _ruleEngineService = ruleEngineService;
    }

    public async Task<GenerateReportResponseDto> GenerateAsync(
        EvaluateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _ruleEngineService.EvaluateAsync(request, cancellationToken);

        var response = new GenerateReportResponseDto
        {
            Success = result.IsValid,
            Message = result.IsValid
                ? "Report rules completed successfully."
                : "Report was stopped because validation failed.",
            WorkflowCode = request.WorkflowCode,
            WorkflowJsonVersion = result.WorkflowJsonVersion,
            GeneratedAtUtc = DateTime.UtcNow,
            ValidationErrors = result.ValidationErrors,
            Calculations = result.Calculations
        };

        foreach (var item in request.Parameters)
        {
            response.Inputs[item.Key] = item.Value.ToString();
        }

        return response;
    }
}
