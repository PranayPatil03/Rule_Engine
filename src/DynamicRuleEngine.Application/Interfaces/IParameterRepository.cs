using DynamicRuleEngine.Domain.Entities;
namespace DynamicRuleEngine.Application.Interfaces;
public interface IParameterRepository
{
    Task<IReadOnlyList<ParameterDefinition>> GetByWorkflowIdAsync(int workflowId, CancellationToken cancellationToken = default);
    Task<ParameterDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(ParameterDefinition entity, CancellationToken cancellationToken = default);
    void Update(ParameterDefinition entity);
    void Remove(ParameterDefinition entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
