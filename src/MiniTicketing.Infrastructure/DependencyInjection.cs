using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Infrastructure.Persistence;
using MiniTicketing.Infrastructure.Persistence.Repositories;

namespace MiniTicketing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'Main' is not configured.");

        services.AddDbContext<MiniTicketingDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(MiniTicketingDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
