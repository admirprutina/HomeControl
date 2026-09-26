using System.Collections.Concurrent;

namespace HomeControl.Application.Messaging;

public sealed class Mediator(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), object> ResponseWrappers = new();
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> NoResponseWrappers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var key = (RequestType: request.GetType(), ResponseType: typeof(TResponse));
        var wrapper = (ResponseRequestHandlerWrapper<TResponse>)ResponseWrappers.GetOrAdd(
            key,
            static key => Activator.CreateInstance(
                typeof(RequestHandlerWrapper<,>).MakeGenericType(key.RequestType, key.ResponseType),
                nonPublic: true)!);

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = NoResponseWrappers.GetOrAdd(
            request.GetType(),
            static requestType => (RequestHandlerWrapper)Activator.CreateInstance(
                typeof(RequestHandlerWrapper<>).MakeGenericType(requestType),
                nonPublic: true)!);

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}
