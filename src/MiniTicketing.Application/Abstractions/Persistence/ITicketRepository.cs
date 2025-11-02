using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);
    Task AddAsync(Ticket ticket, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
    // EF-specifikus “állapot vissza”
    void SetUnchanged(Ticket ticket);
}
