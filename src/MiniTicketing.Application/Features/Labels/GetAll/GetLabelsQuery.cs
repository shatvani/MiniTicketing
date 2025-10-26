using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Labels.GetAll;

public sealed record GetLabelsQuery() : IQuery<Result<IReadOnlyList<LabelResponse>>>;

public sealed class GetLabelsQueryHandler
    : IRequestHandler<GetLabelsQuery, Result<IReadOnlyList<LabelResponse>>>
{
    private readonly IGenericRepository<Label> _labelRepository;

    public GetLabelsQueryHandler(IGenericRepository<Label> labelRepository)
        => _labelRepository = labelRepository;

    public async Task<Result<IReadOnlyList<LabelResponse>>> Handle(GetLabelsQuery request, CancellationToken ct)
    {
        var labels = await _labelRepository.GetAsync(null, x => x.OrderBy(y => y.Name), null, 0, 0, ct);
        var response = labels
            .Select(x => new LabelResponse(x.Id, x.Name))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<LabelResponse>>.Ok(response);
    }
}
