using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.TurnOnLight;

public sealed record TurnOnLightCommandRequest(Guid DeviceId)
    : IRequest<TurnOnLightCommandResult>;
