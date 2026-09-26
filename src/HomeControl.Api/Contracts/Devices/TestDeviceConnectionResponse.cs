namespace HomeControl.Api.Contracts.Devices;

public sealed record TestDeviceConnectionResponse(Guid DeviceId, string Protocol, bool IsSuccessful);
