using HomeControl.Domain.Devices.Events;
using HomeControl.Infrastructure.Persistence.Documents;
using Marten.Events.Aggregation;

namespace HomeControl.Infrastructure.Persistence.Projections;

internal sealed partial class DeviceDocumentProjection : SingleStreamProjection<DeviceDocument, Guid>
{
    public DeviceDocument Create(DeviceRegistered @event)
    {
        return new DeviceDocument
        {
            Id = @event.DeviceId,
            Name = @event.Name,
            Type = @event.Type,
            IsOn = false
        };
    }

    public void Apply(LightTurnedOn @event, DeviceDocument document)
    {
        document.IsOn = true;
    }

    public void Apply(LightTurnedOff @event, DeviceDocument document)
    {
        document.IsOn = false;
    }
}
