using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiniTicketing.Api.Middleware; // ⬅ saját middleware-ek
using MiniTicketing.Infrastructure;
using MiniTicketing.Infrastructure.Persistence;
using FluentValidation;
using MiniTicketing.Application; // ha van Assembly marker
using Serilog;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Core;
// Ha Scrutort használsz a handler scan-hez:
//using Scrutor;
using MiniTicketing.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Serilog (Seq opciósan – ne dőljön el, ha nincs SEQ__URL)
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext();

    var seqUrl = Environment.GetEnvironmentVariable("SEQ__URL");
    if (!string.IsNullOrWhiteSpace(seqUrl) && Uri.IsWellFormedUriString(seqUrl, UriKind.Absolute))
        lc.WriteTo.Seq(seqUrl);
});

var conn =
    builder.Configuration.GetConnectionString("Main")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Main");

builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureApplicationServices(conn);

builder.Services.AddValidatorsFromAssembly(
    typeof(AssemblyMarker).Assembly,            // vagy bármely Application típus assembly-je
    includeInternalTypes: true);

// Mediator + pipeline-ok
builder.Services.AddScoped<IMediator, Mediator>();

// Handlerek felvétele (Scrutorral)
builder.Services.Scan(s => s
    .FromAssemblies(typeof(AssemblyMarker).Assembly)
    .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddOpenApi();
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        // ha akarsz: enumok stringként, stb.
        // o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(o =>
{
    o.AddPolicy("default", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Health checks: self + DB külön taggel
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<MiniTicketingDbContext>("db", tags: new[] { "db" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Pipeline ---
app.UseCorrelationId();         // 0) legyen CorrelationId fejléced + LogContext
app.UseSerilogRequestLogging(); // 1) request log (CorrelationId már a LogContextben)
app.UseProblemDetails();        // 2) egykapus hiba/ProblemDetails

app.UseCors("default");
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/db", new() { Predicate = r => r.Tags.Contains("db") });

app.Run();
