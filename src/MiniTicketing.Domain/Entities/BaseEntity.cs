namespace MiniTicketing.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public abstract class BaseEntity<TKey> : BaseEntity
{
    public required TKey Id { get; set; }
}
