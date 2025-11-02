using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MiniTicketing.Api.Middleware;

public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ProblemDetailsMiddleware(
        RequestDelegate next,
        ILogger<ProblemDetailsMiddleware> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            // ezt akarod látni a Seq-ben is
            _logger.LogWarning(vex, "Validation error on {Path}", context.Request.Path);

            var problem = _problemDetailsFactory.CreateProblemDetails(
                context,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                type: "common.validation_error",
                detail: "One or more validation errors occurred.",
                instance: context.Request.Path);

            // FluentValidation hibák bedrótozása
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            problem.Extensions["errors"] = errors;
            problem.Extensions["correlationId"] = GetCorrelationId(context);

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            // ← EZ MEGY A SEQ-BE STACKKEL
            _logger.LogError(ex,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            // ha már elindult a válasz, nincs mit tenni
            if (context.Response.HasStarted)
            {
                // ilyenkor max. logolni tudsz
                return;
            }

            var problem = _problemDetailsFactory.CreateProblemDetails(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error",
                type: "common.unexpected",
                detail: ex.Message, // DEV-ben oké, PROD-ban akár vedd ki
                instance: context.Request.Path);

            problem.Extensions["correlationId"] = GetCorrelationId(context);

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static string? GetCorrelationId(HttpContext ctx)
    {
        // ahogy te raktad be korábban:
        // app.UseCorrelationId();
        // → vagy headerben, vagy Items-ben lesz
        if (ctx.Response.Headers.TryGetValue("X-Correlation-Id", out var fromResponse))
            return fromResponse.ToString();

        if (ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var fromRequest))
            return fromRequest.ToString();

        // fallback
        return ctx.TraceIdentifier;
    }
}
