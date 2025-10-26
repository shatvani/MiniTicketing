using MiniTicketing.Domain.Enums;

namespace MiniTicketing.Domain.Entities;

public class Ticket : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public Guid ReporterId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDateUtc { get; set; }

    public ICollection<Comment> Comments { get; } = new List<Comment>();
    public ICollection<Label> Labels { get; } = new List<Label>();
    public ICollection<TicketAttachment> TicketAttachments { get; } = new List<TicketAttachment>();
}
