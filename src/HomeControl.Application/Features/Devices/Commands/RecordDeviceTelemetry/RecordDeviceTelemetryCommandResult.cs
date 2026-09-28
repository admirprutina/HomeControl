namespace HomeControl.Application.Features.Devices.Commands.RecordDeviceTelemetry;

public sealed record RecordDeviceTelemetryCommandResult(Guid DeviceId, DateTimeOffset OccurredAtUtc);
