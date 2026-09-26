using HomeControl.Domain.Devices;
using HomeControl.Infrastructure.Connections;

namespace HomeControl.Tests.Connections;

public sealed class DeviceConnectionStrategyResolverTests
{
    private readonly DeviceConnectionStrategyResolver _resolver = new(
        new WifiConnectionStrategy(),
        new ZigbeeConnectionStrategy(),
        new BluetoothConnectionStrategy());

    [Fact]
    public void Light_resolves_wifi()
    {
        Assert.IsType<WifiConnectionStrategy>(_resolver.Resolve(DeviceType.Light));
    }

    [Fact]
    public void Thermostat_resolves_zigbee()
    {
        Assert.IsType<ZigbeeConnectionStrategy>(_resolver.Resolve(DeviceType.Thermostat));
    }

    [Fact]
    public void SmartLock_resolves_bluetooth()
    {
        Assert.IsType<BluetoothConnectionStrategy>(_resolver.Resolve(DeviceType.SmartLock));
    }
}
