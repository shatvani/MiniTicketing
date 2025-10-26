namespace MiniTicketing.Application.Features.Labels.CreateLabel;

using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Application.Features.Labels;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;

public sealed record CreateLabelCommand(string Name) : ICommand<Result<LabelResponse>>;

public sealed class CreateLabelCommandHandler
    : IRequestHandler<CreateLabelCommand, Result<LabelResponse>>
{
    private readonly IGenericRepository<Label> _labelRepository;

    public CreateLabelCommandHandler(IGenericRepository<Label> labelRepository)
        => _labelRepository = labelRepository;

    public async Task<Result<LabelResponse>> Handle(CreateLabelCommand request, CancellationToken ct)
    {
        var normalizedName = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
            return Result<LabelResponse>.Fail(DomainErrorCodes.Label.NameInvalid);

        var exists = await _labelRepository.ExistsAsync(x => x.Name.Equals(normalizedName), ct);
        if (exists)
            return Result<LabelResponse>.Fail(DomainErrorCodes.Label.NameNotUnique);

        var entity = new Label { Id = Guid.NewGuid(), Name = normalizedName };
        await _labelRepository.AddAsync(entity, ct);

        return Result<LabelResponse>.Ok(new LabelResponse(entity.Id, entity.Name));
    }
}
