using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/configuration/workflow")]
public sealed class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _service;

    public WorkflowController(IWorkflowService service)
    {
        _service = service;
    }

    [HttpGet("workflows")]
    public Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflows(CancellationToken cancellationToken)
        => _service.GetWorkflowsAsync(cancellationToken);

    [HttpGet("json")]
    public Task<WorkflowJsonDto> GetJson(
        string workflowCode,
        CancellationToken cancellationToken)
    {
        return _service.GetCurrentJsonAsync(workflowCode, cancellationToken);
    }

    [HttpPut("json")]
    public Task<WorkflowJsonDto> SaveJson(
        string workflowCode,
        [FromBody] WorkflowJsonSaveDto request,
        CancellationToken cancellationToken)
    {
        return _service.SaveJsonAsync(workflowCode, request.JsonContent, cancellationToken);
    }

    [HttpPost("publish")]
    public Task<WorkflowJsonDto> Publish(
        string workflowCode,
        CancellationToken cancellationToken)
    {
        return _service.PublishAsync(workflowCode, cancellationToken);
    }
}
