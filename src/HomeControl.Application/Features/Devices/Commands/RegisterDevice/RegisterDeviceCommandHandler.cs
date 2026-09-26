using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Messaging;
using HomeControl.Domain.Devices;

namespace HomeControl.Application.Features.Devices.Commands.RegisterDevice;

public sealed class RegisterDeviceCommandHandler
    : IRequestHandler<RegisterDeviceCommandRequest, RegisterDeviceCommandResult>
{
    private readonly IDeviceEventStore _eventStore;

    public RegisterDeviceCommandHandler(IDeviceEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<RegisterDeviceCommandResult> Handle(
        RegisterDeviceCommandRequest request,
        CancellationToken cancellationToken)
    {
        var @event = Device.Register(request.Name, request.Type);

        await _eventStore.StartAsync(@event, cancellationToken);

        return new RegisterDeviceCommandResult(@event.DeviceId, @event.Name, @event.Type);
    }
}
