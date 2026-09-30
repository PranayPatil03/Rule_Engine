using DynamicRuleEngine.Application.DTOs;

namespace DynamicRuleEngine.Application.Interfaces;

public interface IReportService
{
    Task<GenerateReportResponseDto> GenerateAsync(
        EvaluateRequestDto request,
        CancellationToken cancellationToken = default);
}
