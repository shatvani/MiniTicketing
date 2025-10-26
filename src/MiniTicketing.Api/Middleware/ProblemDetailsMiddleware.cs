using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MiniTicketing.Api.Middleware;

public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    public ProblemDetailsMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            var problem = new ProblemDetails
            {
                Title = "Validation failed",
                Type = "common.validation_error",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };

            problem.Extensions["errors"] = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            problem.Extensions["correlationId"] = context.Response.Headers["X-Correlation-Id"].ToString();

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
        // Ha lesz saját DomainException-ed (ErrorCode-val), ezt nyisd ki:
        // catch (DomainException dex)
        // {
        //     var problem = new ProblemDetails
        //     {
        //         Title = "Domain error",
        //         Type = dex.ErrorCode, // pl. "label.name_not_unique"
        //         Status = StatusCodes.Status409Conflict,
        //         Instance = context.Request.Path
        //     };
        //     problem.Extensions["correlationId"] = context.Response.Headers["X-Correlation-Id"].ToString();
        //     context.Response.StatusCode = problem.Status!.Value;
        //     context.Response.ContentType = "application/problem+json";
        //     await context.Response.WriteAsJsonAsync(problem);
        // }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Unhandled exception");

            var problem = new ProblemDetails
            {
                Title = "Unexpected error",
                Type = "common.unexpected",
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path
            };
            problem.Extensions["correlationId"] = context.Response.Headers["X-Correlation-Id"].ToString();

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
