using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniTicketing.Application.Abstractions.Persistence;          // IUnitOfWork
using MiniTicketing.Application.Features.Tickets.Shared;           // IAttachmentUpdateOrchestrator
using MiniTicketing.Infrastructure.Persistence;                    // MiniTicketingDbContext, EfUnitOfWork
using MiniTicketing.Infrastructure.Persistence.Repositories;       // GenericRepository<>
using MiniTicketing.Infrastructure.Persistence.Storage;            // MinioFileStorageService, AttachmentStagingService, AttachmentUpdateOrchestrator
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Features.Tickets;
using MiniTicketing.Infrastructure.Persistence.Services;              // IFileStorageService, IAttachmentStagingService

namespace MiniTicketing.Infrastructure;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string 'Main' is missing.", nameof(connectionString));

        // 1) DbContext
        services.AddDbContext<MiniTicketingDbContext>(opt =>
            opt.UseSqlServer(connectionString));

        // ha nagyon akarod a factory-t:
        // services.AddDbContextFactory<MiniTicketingDbContext>(opt => opt.UseSqlServer(connectionString));

        // 2) Generikus repo
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // 3) Unit of Work (EF-es)
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // 4) Storage réteg (MinIO)
        // A MinioOptions-t továbbra is regisztrálhatod az API-ban Configure<MinioOptions>(...)
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IAttachmentStagingService, AttachmentStagingService>();

        // 5) Orchestrator (DB + storage összefűzéséhez)
        services.AddScoped<IAttachmentUpdateOrchestrator, AttachmentUpdateOrchestrator>();

        // 6) Speciális repository-k
        services.AddScoped<ITicketRepository, EfTicketReadRepository>();

        // Ticket listázó service
        services.AddScoped<IListReadService<TicketFilter, TicketDto>, EfTicketReadService>(); // read

        return services;
    }
}
