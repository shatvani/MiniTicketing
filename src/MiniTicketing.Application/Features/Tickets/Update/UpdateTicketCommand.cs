using System.Security;
using Microsoft.Extensions.Logging;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;

namespace MiniTicketing.Application.Features.Tickets.Update;

public sealed record UpdateTicketCommand(TicketUpdateDto ticketDto, List<FileUploadDto> fileUploadDtos) : ICommand<Result<TicketDto>>;

public sealed class UpdateTicketCommandHandler
    : IRequestHandler<UpdateTicketCommand, Result<TicketDto>>
{
  private readonly IGenericRepository<Ticket> _ticketRepository;
  private readonly IGenericRepository<TicketAttachment> _ticketAttachmentRepository;

  private readonly IAttachmentStagingService _attachmentStagingService;
  readonly ILogger<UpdateTicketCommandHandler> _logger;

  public UpdateTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
    IAttachmentStagingService attachmentStagingService,
    ILogger<UpdateTicketCommandHandler> logger,
    IGenericRepository<TicketAttachment> ticketAttachmentRepository)
  {
    _ticketRepository = ticketRepository;
    _ticketAttachmentRepository = ticketAttachmentRepository;
    _attachmentStagingService = attachmentStagingService;
    _logger = logger;
  }

  public async Task<Result<TicketDto>> Handle(UpdateTicketCommand request, CancellationToken ct)
  {
    // Lekéri a módsosítandó jegyet az adatbázisból
    var ticket = await _ticketRepository.GetSingleAsync(
      x => x.Id == request.ticketDto.Id,
      includes: [t => t.TicketAttachments, t => t.Comments, t => t.Labels],
      ct);

    // Hibakezelés: ha a jegy nem létezik, visszatér egy hibával
    if (ticket is null)
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.NotFound);

    // Hibakezelés: érvényes státusz átmenet ellenőrzése
    if (ticket.Status != request.ticketDto.Status)
    {
      var specResult = TicketStatusTransitionAllowed.IsSatisfiedBy(ticket.Status, request.ticketDto.Status);
      if (!specResult.IsSatisfied)
        return Result<TicketDto>.Fail(specResult.ErrorCode ?? DomainErrorCodes.Ticket.InvalidStatusTransition);
    }

    // Hibakezelés: határidő nem lehet múltbeli dátum
    if (request.ticketDto.DueDateUtc != null)
    {
      var dueDateSpecResult = DueDateNotPast.IsSatisfiedBy(request.ticketDto.DueDateUtc.Value);
      if (!dueDateSpecResult.IsSatisfied)
        return Result<TicketDto>.Fail(dueDateSpecResult.ErrorCode ?? DomainErrorCodes.Ticket.DueDateInPast);
    }



    try
    {
      ticket.ApplyChangesFrom(request.ticketDto);
    }
    catch (SecurityException)
    {
      _ticketRepository.SetUnchanged(ticket);
      
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.Conflict);
    }

    // A törlendő fájlok kigyűjtése
    var attachmentsToRemove = ticket.TicketAttachments?.Where(ta => request.ticketDto.RemoveAttachmentIds?.Contains(ta.Id) ?? false).ToList() ?? new List<TicketAttachment>();
 
    List<(string, string, TicketAttachment)> files = new();
    try
    {
      foreach (var attachment in attachmentsToRemove ?? Enumerable.Empty<TicketAttachment>())
      {
        // await RemoveAttachmentFilesAsync(attachment.Path!, ct);
        if (attachment.Path != null)
        {
          await _attachmentStagingService.CopyAttachmentFilesAsync(attachment.Path, $"deleted/{attachment.Path}", ct);
          await _attachmentStagingService.RemoveAttachmentFilesAsync(attachment.Path, ct);
          files.Add((attachment.Path, $"deleted/{attachment.Path}", attachment));
          ticket.TicketAttachments?.Remove(attachment);
        }
      }
    }
    catch
    {
      foreach (var removedFile in files)
      {
        await _attachmentStagingService.CopyAttachmentFilesAsync(removedFile.Item2, removedFile.Item1, ct);
        await _attachmentStagingService.RemoveAttachmentFilesAsync(removedFile.Item2, ct);

        if (ticket.TicketAttachments != null && ticket.TicketAttachments.Any(ta => ta.Id == removedFile.Item3.Id) == false)
        {
          _ticketAttachmentRepository.SetUnchanged(removedFile.Item3);
          ticket.TicketAttachments?.Add(removedFile.Item3);
        }
      }
      _ticketRepository.SetUnchanged(ticket);
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.Conflict);
    } 
    

    // Az új fájlok feltöltéséhez szükséges adatok előkészítése (objectName és FileUploadDto párok)
    var filesToUpload = new List<(string, FileUploadDto)>();
    foreach (var fileUploadDto in request.fileUploadDtos)
    {
      filesToUpload.Add(($"{ticket.Id}/{Guid.NewGuid()}_{fileUploadDto.FileName}", fileUploadDto));
    }

    // Változtatások alkalmazása és adatbázis frissítése
    var newAttachments = Enumerable.Empty<TicketAttachment>();
    try
    {

      newAttachments = request.fileUploadDtos.Select((fileUploadDto, index) => new TicketAttachment
      {
        Id = Guid.NewGuid(),
        TicketId = ticket.Id,
        SizeInBytes = fileUploadDto.Content.Length,
        Path = filesToUpload[index].Item1,
        OriginalFileName = fileUploadDto.FileName,
        MimeType = fileUploadDto.ContentType
      }).ToList();

      foreach (var attachment in newAttachments)
      {
        ticket.TicketAttachments!.Add(attachment);
      }
    }
    catch
    {
      _ticketRepository.SetUnchanged(ticket);
      // Hiba esetén hibával visszatérés
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.Conflict);
    }

    var uploadedFiles = new List<string>();
    try
    {
      // Fájlok feltöltése
      foreach (var fileToUpload in filesToUpload)
      {

        await _attachmentStagingService.AddAttachmentFilesAsync(fileToUpload, ct);
        //_logger.LogInformation("{File} is added", fileToUpload.Item1);
        uploadedFiles.Add(fileToUpload.Item1);
      }
    }
    catch
    {
      foreach (var uploadedFile in uploadedFiles)
      {
        await _attachmentStagingService.RemoveAttachmentFilesAsync($"added/{uploadedFile}", ct);
      }

      foreach (var newAttachment in newAttachments)
      {
        if (ticket.TicketAttachments!.Contains(newAttachment))
        {
          _ticketAttachmentRepository.Detach(newAttachment);
          ticket.TicketAttachments?.Remove(newAttachment);
        }   
      }

      _ticketRepository.SetUnchanged(ticket);
      return Result<TicketDto>.Fail(DomainErrorCodes.Common.Conflict);
    }
    
    foreach (var removedFile in files)
    {
      await _attachmentStagingService.RemoveAttachmentFilesAsync(removedFile.Item2, ct);
    }

    foreach (var uploadedFile in uploadedFiles)
    {
      await _attachmentStagingService.CopyAttachmentFilesAsync($"added/{uploadedFile}", uploadedFile, ct);
      await _attachmentStagingService.RemoveAttachmentFilesAsync($"added/{uploadedFile}", ct);
    }

    await _ticketRepository.SaveChangesAsync(ct);
    
    return Result<TicketDto>.Ok(ticket.ToTicketDto());
  }
}