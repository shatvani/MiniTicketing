using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;

namespace MiniTicketing.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        TResponse? response = default;

        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            response = await next();
        }, ct);

        // itt már commitáltunk
        return response!;
    }
}
