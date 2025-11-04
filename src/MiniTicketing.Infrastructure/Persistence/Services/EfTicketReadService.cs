
using Microsoft.EntityFrameworkCore;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Core;
using MiniTicketing.Application.Features.Tickets;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Services;
public sealed class EfTicketReadService
  : EfListReadServiceBase<Ticket, TicketFilter, TicketDto>
{
  public EfTicketReadService(MiniTicketingDbContext db) : base(db) { }

  protected override IQueryable<Ticket> BuildQuery(TicketFilter f)
  {
    // 1) Mindig sima IQueryable-ként kezdd
    IQueryable<Ticket> q = _db.Tickets.AsNoTracking();

    // 2) Szűrések
    if (f.Status is { } s)     q = q.Where(t => (int)t.Status == s);
    if (f.Priority is { } p)   q = q.Where(t => (int)t.Priority == p);
    if (f.ReporterId is { } r) q = q.Where(t => t.ReporterId == r);

    if (!string.IsNullOrWhiteSpace(f.Search))
    {
      var term = f.Search!;
      // ha kell case-insensitive: EF.Functions.Like
      q = q.Where(t =>
          EF.Functions.Like(t.Title, $"%{term}%") ||
          EF.Functions.Like(t.Description ?? "", $"%{term}%"));
    }

    // 3) Include-okat a végén tedd rá (ha kell a DTO-hoz)
    q = q.Include(t => t.TicketAttachments);
    
    return q;
  }

  protected override IQueryable<Ticket> ApplySort(IQueryable<Ticket> q, IReadOnlyList<SortBy> sort)
  {
    if (sort is null || sort.Count == 0) return q.OrderByDescending(t => t.CreatedAtUtc);

    IOrderedQueryable<Ticket>? ordered = null;

    foreach (var s in sort)
    {
        var (field, desc) = (s.Field.ToLowerInvariant(), s.Desc);

        ordered = (field, desc) switch
        {
            ("createdat", false) => (ordered is null) ? q.OrderBy(t => t.CreatedAtUtc)      : ordered.ThenBy(t => t.CreatedAtUtc),
            ("createdat", true)  => (ordered is null) ? q.OrderByDescending(t => t.CreatedAtUtc) : ordered.ThenByDescending(t => t.CreatedAtUtc),

            ("priority",  false) => (ordered is null) ? q.OrderBy(t => t.Priority)          : ordered.ThenBy(t => t.Priority),
            ("priority",  true)  => (ordered is null) ? q.OrderByDescending(t => t.Priority): ordered.ThenByDescending(t => t.Priority),

            ("status",    false) => (ordered is null) ? q.OrderBy(t => t.Status)            : ordered.ThenBy(t => t.Status),
            ("status",    true)  => (ordered is null) ? q.OrderByDescending(t => t.Status)  : ordered.ThenByDescending(t => t.Status),

            _ => ordered ?? q.OrderByDescending(t => t.CreatedAtUtc) // default fallback
        };
        // fontos: a további körökben már az 'ordered'-et használd
        q = ordered;
    }

    return ordered ?? q;
  }

  protected override IQueryable<TicketDto> Project(IQueryable<Ticket> q)
      => q.Select(t => t.ToTicketDto());
}
