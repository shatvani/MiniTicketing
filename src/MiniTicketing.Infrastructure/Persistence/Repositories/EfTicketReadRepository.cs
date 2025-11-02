using Microsoft.EntityFrameworkCore;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Repositories;

public sealed class EfTicketReadRepository : ITicketRepository
{
  private readonly MiniTicketingDbContext _db;

  public EfTicketReadRepository(MiniTicketingDbContext db)
  {
    _db = db;
  }

  public Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
      => _db.Tickets
            .Include(t => t.TicketAttachments)
            .Include(t => t.Comments)
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

  public Task AddAsync(Ticket ticket, CancellationToken ct)
      => _db.Tickets.AddAsync(ticket, ct).AsTask();

  public Task<int> SaveChangesAsync(CancellationToken ct)
      => _db.SaveChangesAsync(ct);

  public void SetUnchanged(Ticket ticket)
  {
    _db.Entry(ticket).State = EntityState.Unchanged;
  }
}