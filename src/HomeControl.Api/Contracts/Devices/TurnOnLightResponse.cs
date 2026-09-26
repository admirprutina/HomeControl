namespace HomeControl.Api.Contracts.Devices;

public sealed record TurnOnLightResponse(Guid DeviceId, bool IsOn);
