namespace MiniTicketing.Application.Features.Tickets.Shared;

public sealed class AttachmentChangeSet
{
  public IReadOnlyList<AttachmentToAdd> ToAdd { get; init; } = [];
  public IReadOnlyList<TicketAttachment> ToRemove { get; init; } = [];
}

public sealed class AttachmentToAdd
{
  public required TicketAttachment Attachment { get; init; }
  public required FileUploadDto File { get; init; }
}