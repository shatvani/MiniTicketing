using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Api.Requests;

public sealed class UpdateTicketForm
{
    public TicketUpdateDto Ticket { get; set; } = default!;
    public List<IFormFile> Files { get; set; } = new();
}
