namespace MiniTicketing.Application.Features.Tickets;
public class TicketUpdateDto : TicketCreateDto
{
  public Guid Id { get; set; }
  public List<Guid>? KeepAttachmentIds { get; set; } 
  public List<Guid>? RemoveAttachmentIds { get; set; }
}