using HomeControl.Application.Features.Devices.IntegrationEvents;

namespace HomeControl.Application.Abstractions.Messaging;

public interface IDeviceEventPublisher
{
    Task PublishAsync(DeviceLightStateChanged message, CancellationToken cancellationToken);
}
