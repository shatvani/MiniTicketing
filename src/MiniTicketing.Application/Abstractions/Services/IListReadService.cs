using MiniTicketing.Application.Core;

namespace MiniTicketing.Application.Abstractions.Services;

public interface IListReadService<TFilter, TDto>
{
    Task<PagedResult<TDto>> GetPagedAsync(TFilter filter, Paging paging, IReadOnlyList<SortBy> sort, CancellationToken ct);
    IAsyncEnumerable<TDto> StreamAsync(TFilter filter, IReadOnlyList<SortBy> sort, CancellationToken ct);
}