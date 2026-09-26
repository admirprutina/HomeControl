using HomeControl.Application.Abstractions.Connections;
using HomeControl.Domain.Devices;

namespace HomeControl.Infrastructure.Connections;

public sealed class DeviceConnectionStrategyResolver(
    WifiConnectionStrategy wifi,
    ZigbeeConnectionStrategy zigbee,
    BluetoothConnectionStrategy bluetooth) : IDeviceConnectionStrategyResolver
{
    public IDeviceConnectionStrategy Resolve(DeviceType deviceType) => deviceType switch
    {
        DeviceType.Light => wifi,
        DeviceType.Thermostat => zigbee,
        DeviceType.SmartLock => bluetooth,
        _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, "Unsupported device type.")
    };
}
