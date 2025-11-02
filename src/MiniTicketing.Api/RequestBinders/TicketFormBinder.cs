using System.Text.Json;
using MiniTicketing.Api.RequestBinders;
using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Api.RequestBinders;

public sealed class CreateTicketFormBinder : JsonWithFilesBinderBase<TicketCreateDto>
{
  public CreateTicketFormBinder(JsonSerializerOptions options) : base(options) { }
}

public sealed class UpdateTicketFormBinder : JsonWithFilesBinderBase<TicketUpdateDto>
{
    public UpdateTicketFormBinder(JsonSerializerOptions options) : base(options) { }
}