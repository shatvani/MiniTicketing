using System.Text.Json;
using MiniTicketing.Api.Requests;
using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Api.RequestBinders;

public sealed class CreateTicketFormBinder : JsonWithFilesBinderBase<TicketCreateDto>
{
  public CreateTicketFormBinder() : base(DefaultJson.Options) { }
}

public sealed class UpdateTicketFormBinder : JsonWithFilesBinderBase<TicketUpdateDto>
{
    public UpdateTicketFormBinder() : base(DefaultJson.Options) { }
}