using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MiniTicketing.Application.Abstractions;

namespace MiniTicketing.Application.Core;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;

    public Mediator(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var reqType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(reqType, typeof(TResponse));

        // Handler feloldása
        var handler = _sp.GetService(handlerType);
        if (handler is null)
            throw new InvalidOperationException($"No handler registered for {reqType.Name}");

        // Pipeline feloldása (0..n)
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(reqType, typeof(TResponse));
        var behaviors = (_sp.GetServices(behaviorType) ?? Array.Empty<object>()).Reverse().ToArray();

        // Final handler delegate
        RequestHandlerDelegate<TResponse> next = () =>
        {
            var method = handlerType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public)!;
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, ct })!;
        };

        // Fűzzük rá visszafelé a pipeline-t
        foreach (var behavior in behaviors)
        {
            var b = behavior;
            var currentNext = next;
            next = () =>
            {
                var method = behaviorType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public)!;
                return (Task<TResponse>)method.Invoke(b, new object[] { request, ct, currentNext })!;
            };
        }

        return next();
    }
}
