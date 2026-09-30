using DynamicRuleEngine.Application.DTOs;
namespace DynamicRuleEngine.Application.Interfaces;
public interface IRuleService
{
    Task<IReadOnlyList<RuleDto>> GetAsync(string workflowCode, CancellationToken cancellationToken = default);
    Task<RuleDto> CreateAsync(string workflowCode, RuleDto dto, CancellationToken cancellationToken = default);
    Task<RuleDto> UpdateAsync(string workflowCode, RuleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(string workflowCode, int id, CancellationToken cancellationToken = default);
}
