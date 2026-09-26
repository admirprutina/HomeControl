using HomeControl.Domain.Devices;
using HomeControl.Domain.Devices.Events;
using Marten.Events.Aggregation;

namespace HomeControl.Infrastructure.Persistence.Projections;

internal sealed partial class DeviceAggregateProjection : SingleStreamProjection<Device, Guid>
{
    public Device Create(DeviceRegistered @event)
    {
        return Device.Create(@event);
    }

    public void Apply(LightTurnedOn @event, Device device)
    {
        device.Apply(@event);
    }

    public void Apply(LightTurnedOff @event, Device device)
    {
        device.Apply(@event);
    }
}
