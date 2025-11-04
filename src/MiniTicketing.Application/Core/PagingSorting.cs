namespace MiniTicketing.Application.Core;

// Application.Core/PagingSorting.cs
public sealed record Paging(int Page = 1, int PageSize = 50);
public sealed record SortBy(string Field, bool Desc = false);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
