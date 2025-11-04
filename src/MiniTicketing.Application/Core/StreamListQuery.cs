using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Services;

namespace MiniTicketing.Application.Core;

public sealed record StreamListQuery<TFilter, TDto>(TFilter Filter, IReadOnlyList<SortBy> Sort)
  : IQuery<IAsyncEnumerable<TDto>>;

  public sealed class StreamListQueryHandler<TFilter, TDto>
  : IRequestHandler<StreamListQuery<TFilter, TDto>, IAsyncEnumerable<TDto>>
{
  private readonly IListReadService<TFilter, TDto> _svc;
  public StreamListQueryHandler(IListReadService<TFilter, TDto> svc) => _svc = svc;
  public Task<IAsyncEnumerable<TDto>> Handle(StreamListQuery<TFilter, TDto> q, CancellationToken ct)
    => Task.FromResult(_svc.StreamAsync(q.Filter, q.Sort, ct));
}