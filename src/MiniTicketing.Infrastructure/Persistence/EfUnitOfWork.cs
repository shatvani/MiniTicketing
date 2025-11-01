using Microsoft.EntityFrameworkCore;
using MiniTicketing.Application.Abstractions.Persistence;

namespace MiniTicketing.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly MiniTicketingDbContext _dbContext;

    public EfUnitOfWork(MiniTicketingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        // ha már van tranzakció, abba csatlakozunk
        // (pl. másik behavior már nyitott egyet)
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await action(cancellationToken);
            return;
        }

        // különben nyitunk egyet
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action(cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
