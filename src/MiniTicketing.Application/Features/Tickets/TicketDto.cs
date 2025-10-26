using MiniTicketing.Domain.Enums;

namespace MiniTicketing.Application.Features.Tickets;
public class TicketDto
{
  public Guid Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public required TicketStatus Status { get; set; }
  public required PriorityLevel Priority { get; set; }
  public required Guid ReporterId { get; set; }
  public Guid? AssigneeId { get; set; }
  public DateTime? DueDateUtc { get; set; }
}