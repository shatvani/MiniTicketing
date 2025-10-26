namespace MiniTicketing.Application.Features.Labels.GetById;

using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Application.Features.Labels;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;

public sealed record GetLabelByIdQuery(Guid Id) : IQuery<Result<LabelResponse>>;

public sealed class GetLabelByIdQueryHandler
    : IRequestHandler<GetLabelByIdQuery, Result<LabelResponse>>
{
    private readonly IGenericRepository<Label> _labelRepository;

    public GetLabelByIdQueryHandler(IGenericRepository<Label> labelRepository)
        => _labelRepository = labelRepository;

    public async Task<Result<LabelResponse>> Handle(GetLabelByIdQuery request, CancellationToken ct)
    {
        var label = await _labelRepository.GetByIdAsync(request.Id, ct);
        if (label is null)
            return Result<LabelResponse>.Fail(DomainErrorCodes.Common.NotFound);

        return Result<LabelResponse>.Ok(new LabelResponse(label.Id, label.Name));
    }
}
