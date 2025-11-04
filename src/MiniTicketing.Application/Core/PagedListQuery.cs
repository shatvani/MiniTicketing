using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Services;

namespace MiniTicketing.Application.Core;

public sealed record PagedListQuery<TFilter, TDto>(TFilter Filter, Paging Paging, IReadOnlyList<SortBy> Sort)
  : IQuery<PagedResult<TDto>>;

public sealed class PagedListQueryHandler<TFilter, TDto>
  : IRequestHandler<PagedListQuery<TFilter, TDto>, PagedResult<TDto>>
{
  private readonly IListReadService<TFilter, TDto> _svc;
  public PagedListQueryHandler(IListReadService<TFilter, TDto> svc) => _svc = svc;
  public Task<PagedResult<TDto>> Handle(PagedListQuery<TFilter, TDto> q, CancellationToken ct)
    => _svc.GetPagedAsync(q.Filter, q.Paging, q.Sort, ct);
}