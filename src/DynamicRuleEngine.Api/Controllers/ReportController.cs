using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/report")]
public sealed class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateReportResponseDto>> Generate(
        [FromBody] EvaluateRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GenerateAsync(request, cancellationToken);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }
}
