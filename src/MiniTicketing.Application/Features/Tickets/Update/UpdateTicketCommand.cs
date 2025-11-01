using System.Security;
using Microsoft.Extensions.Logging;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Application.Features.Tickets.Shared;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;

namespace MiniTicketing.Application.Features.Tickets.Update;

public sealed record UpdateTicketCommand(TicketUpdateDto ticketDto, List<FileUploadDto> fileUploadDtos) : ICommand<Result<TicketDto>>;

public sealed class UpdateTicketCommandHandler
    : IRequestHandler<UpdateTicketCommand, Result<TicketDto>>
{
  private readonly IGenericRepository<Ticket> _ticketRepository;
  private readonly IAttachmentUpdateOrchestrator _attachmentUpdateOrchestrator;
  private readonly ITicketAttachmentChangeBuilder _attachmentChangeBuilder;

  public UpdateTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
    IAttachmentUpdateOrchestrator attachmentUpdateOrchestrator,
    ITicketAttachmentChangeBuilder attachmentChangeBuilder)
  {
    _ticketRepository = ticketRepository;
    _attachmentUpdateOrchestrator = attachmentUpdateOrchestrator;
    _attachmentChangeBuilder = attachmentChangeBuilder;
  }

  public async Task<Result<TicketDto>> Handle(UpdateTicketCommand request, CancellationToken ct)
  {
    // jegy betöltése
    var ticket = await _ticketRepository.GetSingleAsync(
        x => x.Id == request.ticketDto.Id,
        includes: [t => t.TicketAttachments, t => t.Comments, t => t.Labels],
        ct);

    if (ticket is null)
        return Result<TicketDto>.Fail(DomainErrorCodes.Common.NotFound);

    // domain szabályok ellenőrzése  
    var canApply = ticket.CanApply(
      request.ticketDto.Status,
      request.ticketDto.DueDateUtc);
    if (!canApply.IsSuccess)
    {
        _ticketRepository.SetUnchanged(ticket);
        return Result<TicketDto>.Fail(canApply.ErrorCode ?? DomainErrorCodes.Common.Conflict);
    }
    // mezők másolása (mapperből)
    ticket.ApplyChangesFrom(request.ticketDto);

    // 3. Megnézzük, miket kell törölni / hozzáadni
    var changeSet = _attachmentChangeBuilder.Build(
    ticket,
    request.ticketDto,
    request.fileUploadDtos);

    // 4. Orchestrator meghívása (storage + ticket attachments tisztességesen)
    try
    {
        await _attachmentUpdateOrchestrator.ApplyAsync(ticket, changeSet, ct);
    }
    catch (Exception)
    {
        // itt lehet logolni is, de a mediator logging behavior-ed már úgyis logol
        _ticketRepository.SetUnchanged(ticket);
        return Result<TicketDto>.Fail(DomainErrorCodes.Common.Conflict);
    }

    // 5. DB mentés
    await _ticketRepository.SaveChangesAsync(ct);

    // 6. DTO vissza
    return Result<TicketDto>.Ok(ticket.ToTicketDto());   
  }
}