using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/framework")]
public sealed class FrameworkController : ControllerBase
{
    private readonly IFrameworkService _service;

    public FrameworkController(IFrameworkService service) => _service = service;

    [HttpGet("{workflowCode}")]
    public Task<FrameworkDto> Get(string workflowCode, CancellationToken cancellationToken)
        => _service.GetAsync(workflowCode, cancellationToken);
}
