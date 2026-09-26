using HomeControl.Domain.Devices.Events;

namespace HomeControl.Application.Abstractions.Persistence;

public interface IDeviceEventStore
{
    Task StartAsync(DeviceRegistered @event, CancellationToken cancellationToken);

    Task<IDeviceEventStream?> FetchForWritingAsync(
        Guid deviceId,
        CancellationToken cancellationToken);
}
