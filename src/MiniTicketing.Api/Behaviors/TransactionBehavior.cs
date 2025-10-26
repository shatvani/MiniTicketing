using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Infrastructure.Persistence;

namespace MiniTicketing.Api.Behaviors;

/// <summary>
/// Tranzakciót csak a parancsokra nyitunk (ICommand&lt;T&gt;).
/// A handler NE hívjon SaveChanges-t; itt commitolunk a végén.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly MiniTicketingDbContext _db;

    public TransactionBehavior(MiniTicketingDbContext db) => _db = db;

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        // Query esetén tranzakció nélkül tovább
        var isCommand = typeof(ICommand<TResponse>).IsAssignableFrom(typeof(TRequest));
        if (!isCommand) return await next();

        // Parancs: tranzakció + SaveChanges a végén
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var response = await next();

        if (ShouldCommit(response))
        {
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        else
        {
            await tx.RollbackAsync(ct);
        }

        return response;
    }

    private static bool ShouldCommit(object? response)
    {
        if (response is null) return true;

        if (response is Result result)
            return result.Success;

        var responseType = response.GetType();
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var successProperty = responseType.GetProperty(nameof(Result.Success));
            if (successProperty?.PropertyType == typeof(bool))
            {
                return (bool)successProperty.GetValue(response)!;
            }
        }

        return true;
    }
}
