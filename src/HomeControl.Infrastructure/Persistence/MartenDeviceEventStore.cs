using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Domain.Devices;
using HomeControl.Domain.Devices.Events;
using JasperFx;
using Marten;

namespace HomeControl.Infrastructure.Persistence;

public sealed class MartenDeviceEventStore(IDocumentSession session) : IDeviceEventStore
{
    public async Task StartAsync(DeviceRegistered @event, CancellationToken cancellationToken)
    {
        session.Events.StartStream<Device>(@event.DeviceId, @event);

        try
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw new DeviceConcurrencyException(@event.DeviceId);
        }
    }

    public async Task<IDeviceEventStream?> FetchForWritingAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var stream = await session.Events.FetchForWriting<Device>(deviceId, cancellationToken);

        return stream.Aggregate is null
            ? null
            : new MartenDeviceEventStream(deviceId, stream, session);
    }
}
