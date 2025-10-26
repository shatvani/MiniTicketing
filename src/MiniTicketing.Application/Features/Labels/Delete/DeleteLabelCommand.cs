namespace MiniTicketing.Application.Features.Labels.Delete;


using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;

public sealed record DeleteLabelCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand, Result>
{
    private readonly IGenericRepository<Label> _labelRepository;

    public DeleteLabelCommandHandler(IGenericRepository<Label> labelRepository)
        => _labelRepository = labelRepository;

    public async Task<Result> Handle(DeleteLabelCommand request, CancellationToken ct)
    {
        var label = await _labelRepository.GetByIdAsync(request.Id, ct);
        if (label is null)
            return Result.Fail(DomainErrorCodes.Common.NotFound);

        await _labelRepository.RemoveAsync(label, ct);
        return Result.Ok();
    }
}
