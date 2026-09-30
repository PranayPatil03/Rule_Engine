using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DynamicRuleEngine.Infrastructure.Repositories;

public sealed class WorkflowRepository : IWorkflowRepository
{
    private readonly AppDbContext _db;

    public WorkflowRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<WorkflowDefinition?> GetByCodeAsync(
        string workflowCode,
        CancellationToken cancellationToken = default)
    {
        return _db.Workflows
            .FirstOrDefaultAsync(
                x => x.WorkflowCode == workflowCode && x.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowDefinition>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Workflows
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}