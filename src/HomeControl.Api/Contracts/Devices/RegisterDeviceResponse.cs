namespace HomeControl.Api.Contracts.Devices;

public sealed record RegisterDeviceResponse(Guid DeviceId, string Name, string Type);
