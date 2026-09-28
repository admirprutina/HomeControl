namespace HomeControl.Api.Contracts.Devices;

public sealed record RecordDeviceTelemetryResponse(Guid DeviceId, DateTimeOffset OccurredAtUtc);
