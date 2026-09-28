using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.RecordDeviceTelemetry;

public sealed record RecordDeviceTelemetryCommandRequest(
    Guid DeviceId,
    decimal? TemperatureCelsius,
    decimal? PowerUsageWatts,
    DateTimeOffset? OccurredAtUtc = null) : IRequest<RecordDeviceTelemetryCommandResult>;
