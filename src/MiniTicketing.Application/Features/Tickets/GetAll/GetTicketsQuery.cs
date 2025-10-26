using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;

namespace MiniTicketing.Application.Features.Tickets.GetAll;

public sealed record GetTicketsQuery() : IQuery<Result<IReadOnlyList<TicketDto>>>;

public sealed class GetTicketsQueryHandler
    : IRequestHandler<GetTicketsQuery, Result<IReadOnlyList<TicketDto>>>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;

    public GetTicketsQueryHandler(IGenericRepository<Ticket> ticketRepository)
        => _ticketRepository = ticketRepository;

    public async Task<Result<IReadOnlyList<TicketDto>>> Handle(GetTicketsQuery request, CancellationToken ct)
    {
        var tickets = await _ticketRepository.GetAsync(null, x => x.OrderBy(y => y.Title), null, 0, 0, ct);
        return Result<IReadOnlyList<TicketDto>>.Ok(tickets.ToTicketDtos());
    }
}
