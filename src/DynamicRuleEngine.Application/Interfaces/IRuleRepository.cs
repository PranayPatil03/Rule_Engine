using DynamicRuleEngine.Domain.Entities;
namespace DynamicRuleEngine.Application.Interfaces;
public interface IRuleRepository
{
    Task<IReadOnlyList<RuleDefinition>> GetByWorkflowIdAsync(int workflowId, CancellationToken cancellationToken = default);
    Task<RuleDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(RuleDefinition entity, CancellationToken cancellationToken = default);
    void Update(RuleDefinition entity);
    void Remove(RuleDefinition entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
