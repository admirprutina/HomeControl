using HomeControl.Application.Features.Devices.IntegrationEvents;

namespace HomeControl.Application.Abstractions.Messaging;

public interface IDeviceTelemetryPublisher
{
    Task PublishAsync(DeviceTelemetryRecorded message, CancellationToken cancellationToken);
}
