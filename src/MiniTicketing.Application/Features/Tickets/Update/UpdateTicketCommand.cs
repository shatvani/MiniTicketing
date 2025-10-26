using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;

namespace MiniTicketing.Application.Features.Tickets.Update;

public sealed record UpdateTicketCommand(TicketDto ticketDto) : ICommand<Result<TicketDto>>;

public sealed class UpdateTicketCommandHandler
    : IRequestHandler<UpdateTicketCommand, Result<TicketDto>>
{
  private readonly IGenericRepository<Ticket> _ticketRepository;

  public UpdateTicketCommandHandler(IGenericRepository<Ticket> ticketRepository)
    => _ticketRepository = ticketRepository;

   public async Task<Result<TicketDto>> Handle(UpdateTicketCommand request, CancellationToken ct)
  {
    var ticket = await _ticketRepository.GetByIdAsync(request.ticketDto.Id, ct);
    if (ticket is null)
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.NotFound);

    if (ticket.Status != request.ticketDto.Status)
    {
      var specResult = TicketStatusTransitionAllowed.IsSatisfiedBy(ticket.Status, request.ticketDto.Status);
      if (!specResult.IsSatisfied)
        return Result<TicketDto>.Fail(specResult.ErrorCode ?? DomainErrorCodes.Ticket.InvalidStatusTransition);
    }

    if (request.ticketDto.DueDateUtc != null)
    {
        var dueDateSpecResult = DueDateNotPast.IsSatisfiedBy(request.ticketDto.DueDateUtc.Value);
        if (!dueDateSpecResult.IsSatisfied)
            return Result<TicketDto>.Fail(dueDateSpecResult.ErrorCode ?? DomainErrorCodes.Ticket.DueDateInPast);
    }

    ticket.ApplyChangesFrom(request.ticketDto);

    return Result<TicketDto>.Ok(ticket.ToTicketDto());
  }
}
