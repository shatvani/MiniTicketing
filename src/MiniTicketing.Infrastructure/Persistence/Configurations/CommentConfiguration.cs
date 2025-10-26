using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Comment> b)
    {
        b.ToTable("Comments");

        b.HasKey(x => x.Id);

        b.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(2000);

        b.Property(x => x.AuthorId);

        b.HasIndex(x => new { x.TicketId, x.CreatedAtUtc });
    }
}
