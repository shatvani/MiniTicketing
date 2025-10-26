using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniTicketing.Infrastructure.Persistence;
using MiniTicketing.Domain.Entities;

static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables() // ConnectionStrings__Main
            .Build();

        var conn =
            cfg.GetConnectionString("Main")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Main");

        if (string.IsNullOrWhiteSpace(conn))
        {
            Console.Error.WriteLine("❌ Connection string (ConnectionStrings__Main) not found.");
            return 2;
        }

        Console.WriteLine("➡ Using connection: " + Safe(conn));

        var options = new DbContextOptionsBuilder<MiniTicketingDbContext>()
            .UseSqlServer(conn, sql =>
            {
                sql.MigrationsAssembly(typeof(MiniTicketingDbContext).Assembly.FullName);
            })
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        try
        {
            using var db = new MiniTicketingDbContext(options);

            Console.WriteLine("⏳ Applying migrations…");
            await db.Database.MigrateAsync();
            Console.WriteLine("✅ Migrations applied.");

            Console.WriteLine("⏳ Seeding data…");
            await SeedAsync(db);
            Console.WriteLine("✅ Seeding done.");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("❌ Migration/Seed failed: " + ex);
            return 1;
        }

        static string Safe(string cs) =>
            cs.Replace("Password=", "Password=******", StringComparison.OrdinalIgnoreCase);
    }

    // ---- Seed: idempotens ----
    private static async Task SeedAsync(MiniTicketingDbContext db)
    {
        // Labels: bug, feature, help wanted
        var wanted = new[]
        {
            new Label { Id = Guid.NewGuid(), Name = "bug" },
            new Label { Id = Guid.NewGuid(), Name = "feature" },
            new Label { Id = Guid.NewGuid(), Name = "help wanted" }
        };

        // Case-insensitive egyediség – a DB oldalon Name unique index van,
        // itt pedig nem szúrunk duplát, ha már létezik (LOWER(Name) egyezés).
        foreach (var w in wanted)
        {
            var exists = await db.Labels
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == w.Name.ToLower());

            if (!exists)
            {
                db.Labels.Add(w);
            }
        }

        await db.SaveChangesAsync();
    }
}
