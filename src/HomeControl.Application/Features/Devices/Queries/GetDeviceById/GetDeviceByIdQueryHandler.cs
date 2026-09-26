using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Queries.GetDeviceById;

public sealed class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQueryRequest, GetDeviceByIdQueryResult>
{
    private readonly IDeviceRepository _deviceRepository;

    public GetDeviceByIdQueryHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<GetDeviceByIdQueryResult> Handle(
        GetDeviceByIdQueryRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);

        if (device is null)
        {
            throw new DeviceNotFoundException(request.DeviceId);
        }

        return new GetDeviceByIdQueryResult(device.Id, device.Name, device.Type, device.IsOn);
    }
}
