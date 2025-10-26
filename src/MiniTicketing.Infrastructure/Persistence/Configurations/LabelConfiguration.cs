using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : BaseEntityConfiguration<Label>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Label> b)
    {
        b.ToTable("Labels");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        b.HasIndex(x => x.Name)
            .IsUnique();
    }
}
