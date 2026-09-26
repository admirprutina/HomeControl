using Microsoft.Extensions.DependencyInjection;

namespace HomeControl.Application.Messaging;

internal abstract class ResponseRequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : ResponseRequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        RequestHandlerDelegate<TResponse> next = () => handler.Handle(typedRequest, cancellationToken);

        foreach (var behavior in serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var currentNext = next;
            next = () => behavior.Handle(typedRequest, currentNext, cancellationToken);
        }

        return next();
    }
}

internal abstract class RequestHandlerWrapper
{
    public abstract Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    public override Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        RequestHandlerDelegate next = () => handler.Handle(typedRequest, cancellationToken);

        foreach (var behavior in serviceProvider.GetServices<IPipelineBehavior<TRequest>>().Reverse())
        {
            var currentNext = next;
            next = () => behavior.Handle(typedRequest, currentNext, cancellationToken);
        }

        return next();
    }
}
