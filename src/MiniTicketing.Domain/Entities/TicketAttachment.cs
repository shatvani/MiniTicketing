using MiniTicketing.Domain.Entities;

public class TicketAttachment : BaseEntity<Guid>
{
  public string OriginalFileName { get; set; } = null!;
  public string? Path { get; set; }
  public string MimeType { get; set; } = null!;
  public long SizeInBytes { get; set; }
  public Guid TicketId { get; set; }
  public Ticket Ticket { get; set; } = null!;
}