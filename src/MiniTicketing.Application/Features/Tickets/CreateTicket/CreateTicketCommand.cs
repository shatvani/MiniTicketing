using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;

namespace MiniTicketing.Application.Features.Tickets.CreateTicket;

public sealed record CreateTicketCommand(TicketCreateDto ticketDto, List<FileUploadDto> fileUploadDtos) : ICommand<Result<TicketResponse>>;

public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, Result<TicketResponse>>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IFileStorageService _fileStorage;

    public CreateTicketCommandHandler(IGenericRepository<Ticket> ticketRepository, IFileStorageService fileStorage)
    { 
        _ticketRepository = ticketRepository;
        _fileStorage = fileStorage; 
    }

    public async Task<Result<TicketResponse>> Handle(CreateTicketCommand request, CancellationToken ct)
    {
        if (request.ticketDto.DueDateUtc != null)
        {
            var dueDateSpecResult = DueDateNotPast.IsSatisfiedBy(request.ticketDto.DueDateUtc.Value);
            if (!dueDateSpecResult.IsSatisfied)
                return Result<TicketResponse>.Fail(dueDateSpecResult.ErrorCode ?? DomainErrorCodes.Ticket.DueDateInPast);
        }

        var entity = request.ticketDto.ToNewTicket();
        List<string> savedFilesObjectName = new();
        try {
            if (entity.Id != Guid.Empty)
            {
                foreach (var fileUploadDto in request.fileUploadDtos)
                {
                    var objectName = $"{entity.Id}/{Guid.NewGuid()}_{fileUploadDto.FileName}";
                    savedFilesObjectName.Add(objectName);
                    await _fileStorage.UploadAsync(new MemoryStream(fileUploadDto.Content), objectName, fileUploadDto.ContentType, ct);
                    entity.TicketAttachments.Add(new TicketAttachment
                    {
                        Id = Guid.NewGuid(),
                        TicketId = entity.Id,
                        SizeInBytes = fileUploadDto.Content.Length,
                        Path = objectName,
                        OriginalFileName = fileUploadDto.FileName,
                        MimeType = fileUploadDto.ContentType
                    });
                }
            }       
       
            await _ticketRepository.AddAsync(entity, ct);
        }
        catch 
        {
            foreach (var objectName in savedFilesObjectName) 
            {
                await _fileStorage.DeleteAsync(objectName, ct);
            }
            return Result<TicketResponse>.Fail(DomainErrorCodes.Common.Conflict);
        }        

        return Result<TicketResponse>.Ok(new TicketResponse(entity.Id));
    }
}
