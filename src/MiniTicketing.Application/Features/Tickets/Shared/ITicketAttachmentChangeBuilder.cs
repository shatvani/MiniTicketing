using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Tickets.Shared;

public interface ITicketAttachmentChangeBuilder
{
    // UPDATE
    AttachmentChangeSet Build(Ticket ticket, TicketUpdateDto dto, IReadOnlyList<FileUploadDto> filesToUpload);
    
    // CREATE
    AttachmentChangeSet BuildForCreate(
        Ticket ticket,
        IReadOnlyList<FileUploadDto> filesToUpload);
}
