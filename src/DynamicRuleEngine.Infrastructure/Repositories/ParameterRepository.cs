using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DynamicRuleEngine.Infrastructure.Repositories;

public sealed class ParameterRepository : IParameterRepository
{
    private readonly AppDbContext _db;
    public ParameterRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ParameterDefinition>> GetByWorkflowIdAsync(int workflowId, CancellationToken cancellationToken = default)
        => await _db.Parameters.Include(x => x.ScoreDefinitions).AsNoTracking().Where(x => x.WorkflowId == workflowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

    public Task<ParameterDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Parameters.Include(x => x.ScoreDefinitions).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(ParameterDefinition entity, CancellationToken cancellationToken = default)
        => _db.Parameters.AddAsync(entity, cancellationToken).AsTask();

    public void Update(ParameterDefinition entity) => _db.Parameters.Update(entity);
    public void Remove(ParameterDefinition entity) => _db.Parameters.Remove(entity);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
