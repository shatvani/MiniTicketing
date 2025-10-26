using Microsoft.AspNetCore.Mvc;
using MiniTicketing.Application.Abstractions;
using MiniTicketing.Application.Features.Labels;
using MiniTicketing.Application.Features.Labels.CreateLabel;
using MiniTicketing.Application.Features.Labels.Delete;
using MiniTicketing.Application.Features.Labels.GetAll;
using MiniTicketing.Application.Features.Labels.GetById;
using MiniTicketing.Domain.Errors;

namespace MiniTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabelResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLabelsQuery(), ct);
        if (!result.Success || result.Value is null)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Labels could not be loaded");
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LabelResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLabelByIdQuery(id), ct);
        if (!result.Success || result.Value is null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabelHttpRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLabelCommand(request.Name), ct);

        if (!result.Success)
        {
            var statusCode = result.ErrorCode == DomainErrorCodes.Label.NameInvalid
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status409Conflict;

            return Problem(
                statusCode: statusCode,
                title: "Domain error",
                type: result.ErrorCode,
                instance: HttpContext.Request.Path);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteLabelCommand(id), ct);
        if (!result.Success)
            return NotFound();

        return NoContent();
    }

    public sealed record CreateLabelHttpRequest(string Name);
}
