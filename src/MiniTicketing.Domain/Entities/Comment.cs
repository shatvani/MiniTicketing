namespace MiniTicketing.Domain.Entities;

public class Comment : BaseEntity<Guid>
{
    public Guid TicketId { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
}
