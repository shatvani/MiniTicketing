using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Repositories;

public sealed class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly MiniTicketingDbContext _dbContext;
    private readonly DbSet<TEntity> _set;

    public GenericRepository(MiniTicketingDbContext dbContext)
    {
        _dbContext = dbContext;
        _set = dbContext.Set<TEntity>();
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => _set.AsNoTracking().AnyAsync(predicate, ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
        => _set.AddAsync(entity, ct).AsTask();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int skip = 0, int take = 0, CancellationToken ct = default)
    {
        IQueryable<TEntity> query = _set;
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (take > 0)
        {
            query = query.Skip(skip).Take(take);
        }

        return await query.AsNoTrackingWithIdentityResolution().ToListAsync(ct);
    }

    public void Update(TEntity entity) => _set.Update(entity);
    
    public Task RemoveAsync(TEntity entit, CancellationToken ct = default)
    {
        _set.Remove(entit);
        return Task.CompletedTask;
    }
}
