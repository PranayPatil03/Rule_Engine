using DynamicRuleEngine.Application.DTOs;
namespace DynamicRuleEngine.Application.Interfaces;
public interface IParameterService
{
    Task<IReadOnlyList<ParameterDto>> GetAsync(string workflowCode, CancellationToken cancellationToken = default);
    Task<ParameterDto> CreateAsync(string workflowCode, ParameterDto dto, CancellationToken cancellationToken = default);
    Task<ParameterDto> UpdateAsync(string workflowCode, ParameterDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(string workflowCode, int id, CancellationToken cancellationToken = default);
}
