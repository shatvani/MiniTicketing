using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Tickets.Shared;

public interface ITicketAttachmentChangeBuilder
{
    AttachmentChangeSet Build(Ticket ticket, TicketUpdateDto dto, IReadOnlyList<FileUploadDto> filesToUpload);
}
