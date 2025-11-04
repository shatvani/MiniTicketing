using Microsoft.AspNetCore.Mvc;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Features.Tickets;
using MiniTicketing.Application.Features.Tickets.CreateTicket;
using MiniTicketing.Application.Features.Tickets.GetById;
using MiniTicketing.Application.Features.Tickets.GetAll;
using MiniTicketing.Application.Features.Tickets.Update;
using MiniTicketing.Domain.Errors;
using System.Text.Json;
using MiniTicketing.Api.Requests;
using MiniTicketing.Api.RequestBinders;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Core;
using System.Runtime.CompilerServices;

namespace MiniTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

  public TicketsController(IMediator mediator) 
  {
    _mediator = mediator;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll(CancellationToken ct)
  {
    var result = await _mediator.Send(new GetTicketsQuery(), ct);
    if (!result.Success) return Problem(
          statusCode: MapTicketErrorToStatus(result.ErrorCode),
          title: "Domain error",
          type: result.ErrorCode,
          instance: HttpContext.Request.Path);
    
    return Ok(result.Value ?? Array.Empty<TicketDto>());
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetTicketByIdQuery(id), ct);
    if (!result.Success || result.Value is null)
      return NotFound();

    return Ok(result.Value);
  }

  [HttpGet("gettickets")]
  public Task<PagedResult<TicketDto>> GetTickets(
    [FromQuery] TicketFilter f,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? sort = "createdAt desc",
    CancellationToken ct = default)
  {
    var s = SortParser.Parse(sort); // "createdAt desc,name asc" → List<SortBy>
    return _mediator.Send(new PagedListQuery<TicketFilter, TicketDto>(f, new(page, pageSize), s), ct);
  }
  
  [HttpGet("tickets/stream")]
  public async IAsyncEnumerable<TicketDto> StreamTickets([FromQuery] TicketFilter f, [FromQuery] string? sort = "createdAt desc", [EnumeratorCancellation] CancellationToken ct = default)
  {
      var s = SortParser.Parse(sort);
      var stream = await _mediator.Send(new StreamListQuery<TicketFilter, TicketDto>(f, s), ct);
      await foreach (var dto in stream.WithCancellation(ct)) yield return dto;
  }

  [HttpPost]
  [Consumes("multipart/form-data")]
  // (opcionális) nagy file-okhoz:
  // [RequestSizeLimit(50_000_000)]
  // [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
  public async Task<IActionResult> Create(
    [ModelBinder(typeof(CreateTicketFormBinder))] JsonWithFiles<TicketCreateDto> request,
    CancellationToken ct)
  {
    List<FileUploadDto> fileUploadDto = new();

    foreach (IFormFile file in request.Files)
    {
      if (file.Length > 0)
      {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        fileUploadDto.Add(new FileUploadDto { FileName = file.FileName, Content = memoryStream.ToArray(), ContentType = file.ContentType });
      }
    }

    var result = await _mediator.Send(new CreateTicketCommand(request.Payload, fileUploadDto), ct);

    if (!result.Success)
    {
      return Problem(
          statusCode: MapTicketErrorToStatus(result.ErrorCode),
          title: "Domain error",
          type: result.ErrorCode,
          instance: HttpContext.Request.Path);
    }

    return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
  }
  
  [HttpPut("{id:guid}")]
  public async Task<IActionResult> Update(
    Guid id,
    [ModelBinder(typeof(UpdateTicketFormBinder))] JsonWithFiles<TicketUpdateDto> request,
     CancellationToken ct)
  {
    // 1) route vs body id ellenőrzés
    if (request.Payload.Id == Guid.Empty)
    {
      // ha a kliens nem küldött id-t a JSON-ban, akkor vegyük át a route-ból
      request.Payload.Id = id;
    }
    else if (request.Payload.Id != id)
    {
      // route/body mismatch → 400
      return Problem(
          statusCode: StatusCodes.Status400BadRequest,
          title: "Route/body mismatch",
          type: DomainErrorCodes.Common.ValidationError,
          instance: HttpContext.Request.Path);
    }
    
    List<FileUploadDto> fileUploadDto = new();

    foreach (IFormFile file in request.Files)
    {
      if (file.Length > 0)
      {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        fileUploadDto.Add(new FileUploadDto { FileName = file.FileName, Content = memoryStream.ToArray(), ContentType = file.ContentType });
      }
    }

    var result = await _mediator.Send(new UpdateTicketCommand(request.Payload, fileUploadDto), ct);

    if (!result.Success)
    {
      return Problem(
          statusCode: MapTicketErrorToStatus(result.ErrorCode),
          title: "Domain error",
          type: result.ErrorCode,
          instance: HttpContext.Request.Path);
    }

    return Ok(result.Value);
  }
  
  public sealed record TicketHttpRequest(string TicketJson, IReadOnlyList<IFormFile> Files);

  private static int MapTicketErrorToStatus(string? errorCode) => errorCode switch
  {
      DomainErrorCodes.Common.ValidationError         => StatusCodes.Status400BadRequest,
      DomainErrorCodes.Common.NotFound                => StatusCodes.Status404NotFound,
      DomainErrorCodes.Ticket.InvalidStatusTransition => StatusCodes.Status409Conflict,
      DomainErrorCodes.Ticket.DueDateInPast           => StatusCodes.Status400BadRequest,
      DomainErrorCodes.Ticket.AssigneeRequired        => StatusCodes.Status400BadRequest,
      _                                               => StatusCodes.Status400BadRequest
  };
}
