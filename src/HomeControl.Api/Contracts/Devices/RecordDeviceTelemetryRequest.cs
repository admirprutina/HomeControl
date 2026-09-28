namespace HomeControl.Api.Contracts.Devices;

public sealed record RecordDeviceTelemetryRequest(
    decimal? TemperatureCelsius,
    decimal? PowerUsageWatts,
    DateTimeOffset? OccurredAtUtc = null);
