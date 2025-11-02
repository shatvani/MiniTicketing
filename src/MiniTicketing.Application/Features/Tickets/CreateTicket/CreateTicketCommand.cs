using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Application.Features.Tickets.Shared;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;

namespace MiniTicketing.Application.Features.Tickets.CreateTicket;

public sealed record CreateTicketCommand(TicketCreateDto ticketDto, List<FileUploadDto> fileUploadDtos) : ICommand<Result<TicketResponse>>;

public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, Result<TicketResponse>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAttachmentChangeBuilder _attachmentChangeBuilder;
    private readonly IAttachmentUpdateOrchestrator _attachmentOrchestrator;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketAttachmentChangeBuilder attachmentChangeBuilder,
        IAttachmentUpdateOrchestrator attachmentOrchestrator)
    {
        _ticketRepository = ticketRepository;
        _attachmentChangeBuilder = attachmentChangeBuilder;
        _attachmentOrchestrator = attachmentOrchestrator;
    }

    public async Task<Result<TicketResponse>> Handle(CreateTicketCommand request, CancellationToken ct)
    {
        // 1) domain due date check
        if (request.ticketDto.DueDateUtc is not null)
        {
            var due = DueDateNotPast.IsSatisfiedBy(request.ticketDto.DueDateUtc.Value);
            if (!due.IsSatisfied)
                return Result<TicketResponse>.Fail(due.ErrorCode ?? DomainErrorCodes.Ticket.DueDateInPast);
        }
        // 2) dto -> domain
        var ticket = request.ticketDto.ToNewTicket();

        // 3) attachment changeset create-re
        var changeSet = _attachmentChangeBuilder.BuildForCreate(ticket, request.fileUploadDtos);

        // 4) storage + ticket.TicketAttachments feltöltés
        try
        {
            await _attachmentOrchestrator.ApplyAsync(ticket, changeSet, ct);
        }
        catch
        {
            // itt nincs SetUnchanged, mert új ticket
            return Result<TicketResponse>.Fail(DomainErrorCodes.Common.Conflict);
        }

        // 5) DB mentés
        await _ticketRepository.AddAsync(ticket, ct);
        await _ticketRepository.SaveChangesAsync(ct);       

        return Result<TicketResponse>.Ok(new TicketResponse(ticket.Id));
    }
}
