using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Infrastructure.Persistence.Converters;

namespace MiniTicketing.Infrastructure.Persistence;

public class MiniTicketingDbContext : DbContext
{
    public MiniTicketingDbContext(DbContextOptions<MiniTicketingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Label> Labels => Set<Label>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
                             .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder.Properties<DateTime?>()
                             .HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniTicketingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // Audit mezők automatikus kezelése (BaseEntity-re számítunk)
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.UpdatedAtUtc = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.CreatedAtUtc)).IsModified = false;
                entry.Entity.UpdatedAtUtc = utcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
