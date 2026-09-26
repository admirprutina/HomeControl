using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Commands.TurnOffLight;

public sealed record TurnOffLightCommandRequest(Guid DeviceId)
    : IRequest<TurnOffLightCommandResult>;
