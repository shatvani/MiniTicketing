using Microsoft.AspNetCore.Mvc;
using MiniTicketing.Api.RequestBinders;
using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Api.Requests;

[ModelBinder(typeof(CreateTicketFormBinder))]
public sealed class CreateTicketForm
{
    public TicketCreateDto Ticket { get; set; } = default!;
    public List<IFormFile> Files { get; set; } = new();
}