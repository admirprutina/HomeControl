using HomeControl.Application.Abstractions.Connections;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Queries.TestDeviceConnection;
using HomeControl.Domain.Devices;

namespace HomeControl.Tests.Connections;

public sealed class TestDeviceConnectionQueryHandlerTests
{
    [Fact]
    public async Task Handler_uses_resolved_strategy_for_loaded_device()
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Front door", DeviceType.SmartLock, false);
        var strategy = new RecordingStrategy();
        var resolver = new RecordingResolver(strategy);
        var handler = new TestDeviceConnectionQueryHandler(new StubDeviceRepository(device), resolver);

        var result = await handler.Handle(new TestDeviceConnectionQueryRequest(device.Id), CancellationToken.None);

        Assert.Equal(DeviceType.SmartLock, resolver.RequestedType);
        Assert.Same(device, strategy.TestedDevice);
        Assert.Equal(1, strategy.CallCount);
        Assert.Equal(device.Id, result.DeviceId);
        Assert.Equal("Test protocol", result.Protocol);
        Assert.True(result.IsSuccessful);
    }

    private sealed class StubDeviceRepository(Device device) : IDeviceRepository
    {
        public Task<Device?> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Device?>(device);
        }
    }

    private sealed class RecordingResolver(IDeviceConnectionStrategy strategy) : IDeviceConnectionStrategyResolver
    {
        public DeviceType? RequestedType { get; private set; }

        public IDeviceConnectionStrategy Resolve(DeviceType deviceType)
        {
            RequestedType = deviceType;
            return strategy;
        }
    }

    private sealed class RecordingStrategy : IDeviceConnectionStrategy
    {
        public Device? TestedDevice { get; private set; }

        public int CallCount { get; private set; }

        public ConnectionTestResult TestConnection(Device device)
        {
            TestedDevice = device;
            CallCount++;
            return new ConnectionTestResult("Test protocol", true);
        }
    }
}
