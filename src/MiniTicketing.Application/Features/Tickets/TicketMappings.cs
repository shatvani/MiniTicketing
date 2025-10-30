using MiniTicketing.Application.Features.Tickets.CreateTicket;
using MiniTicketing.Application.Features.Tickets.GetById;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Features.Tickets;

public static class TicketMappings
{
  public static Ticket ToNewTicket(this TicketCreateDto ticketDto)
      => new Ticket
      {
        Id = Guid.NewGuid(),
        Title = ticketDto.Title,
        Description = ticketDto.Description,
        Status = ticketDto.Status,
        Priority = ticketDto.Priority,
        ReporterId = ticketDto.ReporterId,
        AssigneeId = ticketDto.AssigneeId,
        DueDateUtc = ticketDto.DueDateUtc
      };

  public static TicketDto ToTicketDto(this Ticket ticket)
     => new TicketDto
     {
       Id = ticket.Id,
       Title = ticket.Title,
       Description = ticket.Description,
       Status = ticket.Status,
       Priority = ticket.Priority,
       ReporterId = ticket.ReporterId,
       AssigneeId = ticket.AssigneeId,
       DueDateUtc = ticket.DueDateUtc
     };

  public static IReadOnlyList<TicketDto> ToTicketDtos(this IEnumerable<Ticket> tickets) =>
     tickets.Select(t => t.ToTicketDto()).ToList();
        
  public static void ApplyChangesFrom(this Ticket ticket, TicketUpdateDto dto)
  {
      ticket.Title = dto.Title!.Trim();
      ticket.Description = dto.Description?.Trim();
      ticket.Status = dto.Status;
      ticket.Priority = dto.Priority;
      ticket.ReporterId = dto.ReporterId;
      ticket.AssigneeId = dto.AssigneeId;
      ticket.DueDateUtc = dto.DueDateUtc;
  }
}