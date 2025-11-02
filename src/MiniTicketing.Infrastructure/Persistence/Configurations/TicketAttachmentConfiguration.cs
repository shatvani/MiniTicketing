using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Configurations;

public class TicketAttachmentConfiguration : BaseEntityConfiguration<TicketAttachment>
{
  protected override void ConfigureEntity(EntityTypeBuilder<TicketAttachment> b)
  {
        b.ToTable("TicketAttachments");

        b.HasKey(x => x.Id);

        b.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(500);

        b.Property(x => x.SizeInBytes)    
            .IsRequired();

        b.Property(x => x.TicketId)
            .IsRequired();      
        
        b.HasOne(x => x.Ticket)
            .WithMany(t => t.TicketAttachments)
            .HasForeignKey(x => x.TicketId)
            // erről mindjárt beszélünk
            .OnDelete(DeleteBehavior.NoAction);
    }
}
