using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : BaseEntityConfiguration<Ticket>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("Tickets");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Description)
            .HasMaxLength(4000);

        b.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.Priority)
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.ReporterId);
        b.Property(x => x.AssigneeId);

        b.Property(x => x.DueDateUtc);

        b.HasIndex(x => new { x.Status, x.Priority });
        b.HasIndex(x => x.AssigneeId);
        b.HasIndex(x => x.CreatedAtUtc);

        b.HasMany(x => x.Comments)
            .WithOne()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Labels)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TicketLabels",
                right => right.HasOne<Label>()
                              .WithMany()
                              .HasForeignKey("LabelId")
                              .HasConstraintName("FK_TicketLabels_Labels_LabelId")
                              .OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Ticket>()
                              .WithMany()
                              .HasForeignKey("TicketId")
                              .HasConstraintName("FK_TicketLabels_Tickets_TicketId")
                              .OnDelete(DeleteBehavior.Cascade),
                je =>
                {
                    je.ToTable("TicketLabels");
                    je.HasKey("TicketId", "LabelId");
                    je.HasIndex("LabelId");
                });
    }
}
