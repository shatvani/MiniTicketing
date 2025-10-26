using System.Linq.Expressions;

namespace MiniTicketing.Application.Abstractions.Persistence;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int skip = 0, int take = 0, CancellationToken ct = default);
    void Update(TEntity entity);
    Task RemoveAsync(TEntity entit, CancellationToken ct = default);
}
