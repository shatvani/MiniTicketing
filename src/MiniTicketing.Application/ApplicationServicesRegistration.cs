using Microsoft.Extensions.DependencyInjection;
using MiniTicketing.Application.Abstractions;                       // IRequest<>, IPipelineBehavior<,>
using MiniTicketing.Application.Behaviors;                         // ValidationBehavior, LoggingBehavior, TransactionBehavior
using MiniTicketing.Application.Features.Tickets.Shared;           // ITicketAttachmentChangeBuilder, TicketAttachmentChangeBuilder

namespace MiniTicketing.Application;

public static class ApplicationServicesRegistration
{
  public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
    
    services.AddScoped<ITicketAttachmentChangeBuilder, TicketAttachmentChangeBuilder>();
    return services;
  }
}