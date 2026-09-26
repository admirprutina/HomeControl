using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.TurnOnLight;

public sealed class TurnOnLightCommandHandler(IDeviceEventStore eventStore)
    : IRequestHandler<TurnOnLightCommandRequest, TurnOnLightCommandResult>
{
    public async Task<TurnOnLightCommandResult> Handle(
        TurnOnLightCommandRequest request,
        CancellationToken cancellationToken)
    {
        var stream = await eventStore.FetchForWritingAsync(request.DeviceId, cancellationToken);

        if (stream is null)
        {
            throw new DeviceNotFoundException(request.DeviceId);
        }

        var @event = stream.Aggregate.TurnOn();
        stream.Append(@event);
        await stream.SaveChangesAsync(cancellationToken);

        return new TurnOnLightCommandResult(request.DeviceId, stream.Aggregate.IsOn);
    }
}
