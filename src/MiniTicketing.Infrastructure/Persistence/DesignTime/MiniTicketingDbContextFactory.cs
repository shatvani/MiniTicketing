using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MiniTicketing.Infrastructure.Persistence;

public sealed class MiniTicketingDbContextFactory
    : IDesignTimeDbContextFactory<MiniTicketingDbContext>
{
    public MiniTicketingDbContext CreateDbContext(string[] args)
    {
        // 1) Konfig építése: először az Api appsettings-e (ha elérhető),
        //    majd env vars, végül egy biztonságos fallback.
        var configuration = BuildConfiguration();

        var conn =
            configuration.GetConnectionString("Main")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Main")
            ?? "Server=host.docker.internal,1433;Database=MiniTicketing;User Id=miniapp;Password=YourStrong!Passw0rd;TrustServerCertificate=true";

        var optionsBuilder = new DbContextOptionsBuilder<MiniTicketingDbContext>()
            .UseSqlServer(conn, sql =>
            {
                // Migrációk assembly-je = Infrastructure
                sql.MigrationsAssembly(typeof(MiniTicketingDbContext).Assembly.FullName);
                // Ha kell, itt állíthatsz CommandTimeoutot, retry-t, stb.
            });

        // Only for design-time developer convenience: enable detailed errors & sensitive data logging
        var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
        }

        return new MiniTicketingDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        // Kiindulás: Infrastructure projekt mappa
        var infraDir = Directory.GetCurrentDirectory();

        // Megpróbáljuk az Api projekt gyökerét: ../MiniTicketing.Api
        var apiDir = Path.GetFullPath(Path.Combine(infraDir, "..", "MiniTicketing.Api"));

        var cb = new ConfigurationBuilder()
            .AddEnvironmentVariables(); // ConnectionStrings__Main, ASPNETCORE_ENVIRONMENT, stb.

        // Ha létezik az Api mappa, betöltjük az (appsettings).json-okat is onnan
        if (Directory.Exists(apiDir))
        {
            cb.SetBasePath(apiDir)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
              .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
        }

        return cb.Build();
    }
}
