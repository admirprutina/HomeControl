namespace HomeControl.Application.Features.Devices.Commands.TurnOnLight;

public sealed record TurnOnLightCommandResult(Guid DeviceId, bool IsOn);
