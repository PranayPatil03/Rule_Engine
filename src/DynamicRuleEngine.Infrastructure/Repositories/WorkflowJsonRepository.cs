using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DynamicRuleEngine.Infrastructure.Repositories;

public sealed class WorkflowJsonRepository : IWorkflowJsonRepository
{
    private readonly AppDbContext _db;

    public WorkflowJsonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkflowJson?> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken = default)
    {
        return await _db.WorkflowJsons
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.WorkflowId == workflowId,
                cancellationToken);
    }

    public async Task UpsertAsync(
        WorkflowJson entity,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.WorkflowJsons
            .FirstOrDefaultAsync(
                x => x.WorkflowId == entity.WorkflowId,
                cancellationToken);

        if (existing is null)
        {
            await _db.WorkflowJsons.AddAsync(
                entity,
                cancellationToken);
        }
        else
        {
            existing.Version = entity.Version;
            existing.SourceHash = entity.SourceHash;
            existing.JsonContent = entity.JsonContent;
            existing.GeneratedAtUtc = entity.GeneratedAtUtc;

            _db.WorkflowJsons.Update(existing);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}