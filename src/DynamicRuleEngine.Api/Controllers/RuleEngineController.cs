using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/rule-engine")]
public sealed class RuleEngineController : ControllerBase
{
    private readonly IRuleEngineService _service;

    public RuleEngineController(IRuleEngineService service)
    {
        _service = service;
    }

    [HttpPost("evaluate")]
    public Task<EvaluateResponseDto> Evaluate(
        [FromBody] EvaluateRequestDto request,
        CancellationToken cancellationToken)
        => _service.EvaluateAsync(request, cancellationToken);

    [HttpPost("evaluate-framework")]
    public Task<EvaluateResponseDto> EvaluateFramework(
        [FromBody] FrameworkEvaluationRequestDto request,
        CancellationToken cancellationToken)
        => _service.EvaluateFrameworkAsync(request, cancellationToken);
}
