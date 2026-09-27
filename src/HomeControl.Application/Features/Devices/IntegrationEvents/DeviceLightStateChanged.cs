namespace HomeControl.Application.Features.Devices.IntegrationEvents;

public sealed record DeviceLightStateChanged(Guid DeviceId, bool IsOn, DateTimeOffset OccurredAtUtc);
