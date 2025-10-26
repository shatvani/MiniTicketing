namespace MiniTicketing.Domain.Entities;

public class Label : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}
