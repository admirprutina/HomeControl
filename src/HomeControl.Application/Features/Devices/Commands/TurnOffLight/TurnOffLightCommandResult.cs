namespace HomeControl.Application.Features.Devices.Commands.TurnOffLight;

public sealed record TurnOffLightCommandResult(Guid DeviceId, bool IsOn);
