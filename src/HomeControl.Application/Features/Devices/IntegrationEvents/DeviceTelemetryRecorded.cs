namespace HomeControl.Application.Features.Devices.IntegrationEvents;

public sealed record DeviceTelemetryRecorded(
    Guid DeviceId,
    decimal? TemperatureCelsius,
    decimal? PowerUsageWatts,
    DateTimeOffset OccurredAtUtc);
