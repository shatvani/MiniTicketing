using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Tickets.Shared;

public sealed class TicketAttachmentChangeBuilder : ITicketAttachmentChangeBuilder
{
    public AttachmentChangeSet Build(
        Ticket ticket,
        TicketUpdateDto dto,
        IReadOnlyList<FileUploadDto> filesToUpload)
    {
        // 1) melyeket kell törölni?
        var toRemove = ticket.TicketAttachments?
            .Where(ta => dto.RemoveAttachmentIds?.Contains(ta.Id) ?? false)
            .ToList()
            ?? new List<TicketAttachment>();

        // 2) melyeket kell hozzáadni?
        var newAttachments = new List<AttachmentToAdd>();

        foreach (var file in filesToUpload)
        {
            var attachment = new TicketAttachment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                SizeInBytes = file.Content.Length,
                Path = $"{ticket.Id}/{Guid.NewGuid()}_{file.FileName}",
                OriginalFileName = file.FileName,
                MimeType = file.ContentType
            };

            newAttachments.Add(new AttachmentToAdd
            {
                Attachment = attachment,
                File = file
            });
        }

        return new AttachmentChangeSet
        {
            ToRemove = toRemove,
            ToAdd = newAttachments
        };
    }
}
