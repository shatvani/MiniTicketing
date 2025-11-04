using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MiniTicketing.Application.Abstractions;                       // IRequest<>, IPipelineBehavior<,>
using MiniTicketing.Application.Behaviors;                         // ValidationBehavior, LoggingBehavior, TransactionBehavior
using MiniTicketing.Application.Features.Tickets.Shared;           // ITicketAttachmentChangeBuilder, TicketAttachmentChangeBuilder
using MiniTicketing.Application.Common.Results;
using MiniTicketing.Application.Features.Labels;
using MiniTicketing.Application.Features.Labels.CreateLabel;
using MiniTicketing.Application.Features.Labels.Delete;
using MiniTicketing.Application.Features.Labels.GetAll;
using MiniTicketing.Application.Features.Labels.GetById;
using MiniTicketing.Application.Features.Tickets;
using MiniTicketing.Application.Features.Tickets.CreateTicket;
using MiniTicketing.Application.Features.Tickets.GetAll;
using MiniTicketing.Application.Features.Tickets.GetById;
using MiniTicketing.Application.Features.Tickets.Update;
using MiniTicketing.Application.Core;

namespace MiniTicketing.Application;

public static class ApplicationServicesRegistration
{
  public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    services.AddScoped<ITicketAttachmentChangeBuilder, TicketAttachmentChangeBuilder>();

    services.AddScoped<IRequestHandler<GetTicketsQuery, Result<IReadOnlyList<TicketDto>>>, GetTicketsQueryHandler>();
    services.AddScoped<IRequestHandler<GetTicketByIdQuery, Result<TicketDto>>, GetTicketByIdQueryHandler>();
    services.AddScoped<IRequestHandler<CreateTicketCommand, Result<TicketResponse>>, CreateTicketCommandHandler>();
    services.AddScoped<IRequestHandler<UpdateTicketCommand, Result<TicketDto>>, UpdateTicketCommandHandler>();

    services.AddScoped<IRequestHandler<GetLabelsQuery, Result<IReadOnlyList<LabelResponse>>>, GetLabelsQueryHandler>();
    services.AddScoped<IRequestHandler<GetLabelByIdQuery, Result<LabelResponse>>, GetLabelByIdQueryHandler>();
    services.AddScoped<IRequestHandler<CreateLabelCommand, Result<LabelResponse>>, CreateLabelCommandHandler>();
    services.AddScoped<IRequestHandler<DeleteLabelCommand, Result>, DeleteLabelCommandHandler>();

    services.AddScoped<IRequestHandler<PagedListQuery<TicketFilter, TicketDto>, PagedResult<TicketDto>>, PagedListQueryHandler<TicketFilter, TicketDto>>();
    services.AddScoped<IRequestHandler<StreamListQuery<TicketFilter, TicketDto>, IAsyncEnumerable<TicketDto>>, StreamListQueryHandler<TicketFilter, TicketDto>>();
    return services;
  }
}
