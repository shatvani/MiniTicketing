using Microsoft.EntityFrameworkCore;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Core;
using MiniTicketing.Infrastructure.Persistence;

namespace MiniTicketing.Infrastructure.Persistence.Services;

public abstract class EfListReadServiceBase<TEntity, TFilter, TDto> : IListReadService<TFilter, TDto>
    where TEntity : class
{
    protected readonly MiniTicketingDbContext _db;
    protected EfListReadServiceBase(MiniTicketingDbContext db) => _db = db;

    protected abstract IQueryable<TEntity> BuildQuery(TFilter filter);
    protected abstract IQueryable<TEntity> ApplySort(IQueryable<TEntity> q, IReadOnlyList<SortBy> sort);
    protected abstract IQueryable<TDto> Project(IQueryable<TEntity> q);

    public async Task<PagedResult<TDto>> GetPagedAsync(TFilter filter, Paging paging, IReadOnlyList<SortBy> sort, CancellationToken ct)
    {
        var q = ApplySort(BuildQuery(filter), sort);
        var total = await q.CountAsync(ct);
        var items = await Project(q)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new(items, total, paging.Page, paging.PageSize);
    }

    public async IAsyncEnumerable<TDto> StreamAsync(TFilter filter, IReadOnlyList<SortBy> sort, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var q = ApplySort(BuildQuery(filter), sort);
        var projected = Project(q).AsAsyncEnumerable();
        await foreach (var dto in projected.WithCancellation(ct)) yield return dto;
    }
}
