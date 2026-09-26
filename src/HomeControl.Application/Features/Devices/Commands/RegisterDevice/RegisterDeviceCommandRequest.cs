using HomeControl.Application.Messaging;
using HomeControl.Domain.Devices;

namespace HomeControl.Application.Features.Devices.Commands.RegisterDevice;

public sealed record RegisterDeviceCommandRequest(string Name, DeviceType Type)
    : IRequest<RegisterDeviceCommandResult>;
