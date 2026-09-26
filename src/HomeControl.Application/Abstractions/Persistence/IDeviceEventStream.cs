using HomeControl.Domain.Devices;

namespace HomeControl.Application.Abstractions.Persistence;

public interface IDeviceEventStream
{
    Device Aggregate { get; }

    void Append(object @event);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
