using HomeControl.Application.Abstractions.Messaging;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.RecordDeviceTelemetry;

public sealed class RecordDeviceTelemetryCommandHandler(
    IDeviceRepository repository,
    IDeviceTelemetryPublisher publisher)
    : IRequestHandler<RecordDeviceTelemetryCommandRequest, RecordDeviceTelemetryCommandResult>
{
    public async Task<RecordDeviceTelemetryCommandResult> Handle(
        RecordDeviceTelemetryCommandRequest request,
        CancellationToken cancellationToken)
    {
        var device = await repository.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device is null)
        {
            throw new DeviceNotFoundException(request.DeviceId);
        }

        var message = new DeviceTelemetryRecorded(
            device.Id,
            request.TemperatureCelsius,
            request.PowerUsageWatts,
            request.OccurredAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow);
        await publisher.PublishAsync(message, cancellationToken);

        return new RecordDeviceTelemetryCommandResult(message.DeviceId, message.OccurredAtUtc);
    }
}
