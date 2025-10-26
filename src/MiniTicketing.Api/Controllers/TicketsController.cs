using Microsoft.AspNetCore.Mvc;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Features.Tickets;
using MiniTicketing.Application.Features.Tickets.CreateTicket;
using MiniTicketing.Application.Features.Tickets.GetById;
using MiniTicketing.Application.Features.Tickets.GetAll;
using MiniTicketing.Application.Features.Tickets.Update;
using MiniTicketing.Domain.Errors;
using System.Text.Json;

namespace MiniTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

  public TicketsController(IMediator mediator) => _mediator = mediator;

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

  [HttpPost]
  [Consumes("multipart/form-data")]
  // (opcionális) nagy file-okhoz:
  // [RequestSizeLimit(50_000_000)]
  // [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
  public async Task<IActionResult> Create([FromForm] TicketHttpRequest form, CancellationToken ct)
  {
    TicketDto? ticketDto;
    try
    {
      ticketDto = JsonSerializer.Deserialize<TicketDto>(form.TicketJson, _jsonOptions);
    }
    catch (JsonException)
    {
      return Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Route/body mismatch",
        type: DomainErrorCodes.Common.ValidationError,
        instance: HttpContext.Request.Path);
    }

    if (ticketDto is null)
    {
      return Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Invalid ticket JSON",
        type: DomainErrorCodes.Common.ValidationError,
        instance: HttpContext.Request.Path);
    }

    List<FileUploadDto> fileUploadDto = new();

    foreach (IFormFile file in form.Files)
    {
      if (file.Length > 0)
      {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        fileUploadDto.Add(new FileUploadDto { FileName = file.FileName, Content = memoryStream.ToArray(), ContentType = file.ContentType });
      }
    }

    var result = await _mediator.Send(new CreateTicketCommand(ticketDto, fileUploadDto), ct);

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
  public async Task<IActionResult> Update(Guid id, [FromForm] TicketHttpRequest form, CancellationToken ct)
  {
    TicketDto? ticketDto;
    try
    {
      ticketDto = JsonSerializer.Deserialize<TicketDto>(form.TicketJson, _jsonOptions);
    }
    catch (JsonException)
    {
      return Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Route/body mismatch",
        type: DomainErrorCodes.Common.ValidationError,
        instance: HttpContext.Request.Path);
    }

    if (ticketDto is null)
    {
      return Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Invalid ticket JSON",
        type: DomainErrorCodes.Common.ValidationError,
        instance: HttpContext.Request.Path);
    }
  

    var result = await _mediator.Send(new UpdateTicketCommand(ticketDto), ct);

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
