namespace HomeControl.Api.Contracts.Devices;

public sealed record TurnOffLightResponse(Guid DeviceId, bool IsOn);
