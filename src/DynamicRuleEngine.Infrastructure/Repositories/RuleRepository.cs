using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DynamicRuleEngine.Infrastructure.Repositories;

public sealed class RuleRepository : IRuleRepository
{
    private readonly AppDbContext _db;
    public RuleRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RuleDefinition>> GetByWorkflowIdAsync(int workflowId, CancellationToken cancellationToken = default)
        => await _db.Rules.AsNoTracking().Where(x => x.WorkflowId == workflowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

    public Task<RuleDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Rules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(RuleDefinition entity, CancellationToken cancellationToken = default)
        => _db.Rules.AddAsync(entity, cancellationToken).AsTask();

    public void Update(RuleDefinition entity) => _db.Rules.Update(entity);
    public void Remove(RuleDefinition entity) => _db.Rules.Remove(entity);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
