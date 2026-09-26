using HomeControl.Application.Abstractions.Connections;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Queries.TestDeviceConnection;

public sealed class TestDeviceConnectionQueryHandler(
    IDeviceRepository deviceRepository,
    IDeviceConnectionStrategyResolver strategyResolver)
    : IRequestHandler<TestDeviceConnectionQueryRequest, TestDeviceConnectionQueryResult>
{
    public async Task<TestDeviceConnectionQueryResult> Handle(
        TestDeviceConnectionQueryRequest request,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken)
            ?? throw new DeviceNotFoundException(request.DeviceId);

        var strategy = strategyResolver.Resolve(device.Type);
        var result = strategy.TestConnection(device);

        return new TestDeviceConnectionQueryResult(device.Id, result.Protocol, result.IsSuccessful);
    }
}
