using DynamicRuleEngine.Application.DTOs;
using DynamicRuleEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRuleEngine.Api.Controllers;

[ApiController]
[Route("api/configuration/rules")]
public sealed class RulesController : ControllerBase
{
    private readonly IRuleService _service;
    public RulesController(IRuleService service) => _service = service;

    [HttpGet]
    public Task<IReadOnlyList<RuleDto>> Get(string workflowCode, CancellationToken cancellationToken)
        => _service.GetAsync(workflowCode, cancellationToken);

    [HttpPost]
    public Task<RuleDto> Create(string workflowCode, [FromBody] RuleDto dto, CancellationToken cancellationToken)
        => _service.CreateAsync(workflowCode, dto, cancellationToken);

    [HttpPut("{id:int}")]
    public Task<RuleDto> Update(string workflowCode, int id, [FromBody] RuleDto dto, CancellationToken cancellationToken)
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
