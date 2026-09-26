namespace HomeControl.Api.Contracts.Devices;

public sealed record GetDeviceByIdResponse(Guid DeviceId, string Name, string Type, bool IsOn);
