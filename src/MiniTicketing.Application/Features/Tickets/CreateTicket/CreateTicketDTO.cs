using MiniTicketing.Domain.Enums;

namespace MiniTicketing.Application.Features.Tickets.CreateTicket;

public class CreateTicketDto
{
  public required string Title { get; set; }
  public required string Description { get; set; }
  public TicketStatus Status { get; init; }
  public PriorityLevel Priority { get; set; }
  public required Guid ReporterId { get; set; }
  public Guid? AssigneeId { get; set; }
  public DateTime? DueDateUtc { get; set; }
}