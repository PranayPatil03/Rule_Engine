using DynamicRuleEngine.Application.DTOs;

namespace DynamicRuleEngine.Application.Interfaces;

public interface IRuleEngineService
{
    Task<EvaluateResponseDto> EvaluateAsync(
        EvaluateRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EvaluateResponseDto> EvaluateFrameworkAsync(
        FrameworkEvaluationRequestDto request,
        CancellationToken cancellationToken = default);

    void Invalidate(string workflowCode, int version);
}
