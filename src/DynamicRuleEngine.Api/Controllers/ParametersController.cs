using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/configuration/parameters")]
public sealed class ParametersController : ControllerBase
{
    private readonly IParameterService _service;
    public ParametersController(IParameterService service) => _service = service;

    [HttpGet]
    public Task<IReadOnlyList<ParameterDto>> Get(string workflowCode, CancellationToken cancellationToken)
        => _service.GetAsync(workflowCode, cancellationToken);

    [HttpPost]
    public Task<ParameterDto> Create(string workflowCode, [FromBody] ParameterDto dto, CancellationToken cancellationToken)
        => _service.CreateAsync(workflowCode, dto, cancellationToken);

    [HttpPut("{id:int}")]
    public Task<ParameterDto> Update(string workflowCode, int id, [FromBody] ParameterDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return _service.UpdateAsync(workflowCode, dto, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string workflowCode, int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(workflowCode, id, cancellationToken);
        return NoContent();
    }
}
