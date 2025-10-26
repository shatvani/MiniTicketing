using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;

namespace MiniTicketing.Application.Features.Tickets.GetById;

public sealed record GetTicketByIdQuery(Guid Id) : IQuery<Result<TicketDto>>;

public sealed class GetTicketByIdQueryHandler
    : IRequestHandler<GetTicketByIdQuery, Result<TicketDto>>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;

    public GetTicketByIdQueryHandler(IGenericRepository<Ticket> ticketRepository)
        => _ticketRepository = ticketRepository;

    public async Task<Result<TicketDto>> Handle(GetTicketByIdQuery request, CancellationToken ct)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, ct);
        if (ticket is null)
            return Result<TicketDto>.Fail(DomainErrorCodes.Common.NotFound);

        return Result<TicketDto>.Ok(ticket.ToTicketDto());
    }
}
