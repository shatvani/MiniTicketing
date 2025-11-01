using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Tickets.Shared;

public interface IAttachmentUpdateOrchestrator
{
    Task ApplyAsync(Ticket ticket, AttachmentChangeSet changeSet, CancellationToken ct);
}
