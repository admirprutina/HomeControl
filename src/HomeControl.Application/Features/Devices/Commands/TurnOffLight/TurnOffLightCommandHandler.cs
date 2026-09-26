using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.TurnOffLight;

public sealed class TurnOffLightCommandHandler(IDeviceEventStore eventStore)
    : IRequestHandler<TurnOffLightCommandRequest, TurnOffLightCommandResult>
{
    public async Task<TurnOffLightCommandResult> Handle(
        TurnOffLightCommandRequest request,
        CancellationToken cancellationToken)
    {
        var stream = await eventStore.FetchForWritingAsync(request.DeviceId, cancellationToken);

        if (stream is null)
        {
            throw new DeviceNotFoundException(request.DeviceId);
        }

        var @event = stream.Aggregate.TurnOff();
        stream.Append(@event);
        await stream.SaveChangesAsync(cancellationToken);

        return new TurnOffLightCommandResult(request.DeviceId, stream.Aggregate.IsOn);
    }
}
