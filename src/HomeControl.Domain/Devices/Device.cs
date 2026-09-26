using HomeControl.Domain.Devices.Exceptions;
using HomeControl.Domain.Devices.Events;

namespace HomeControl.Domain.Devices;

public sealed class Device
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public DeviceType Type { get; private set; }

    public bool IsOn { get; private set; }

    private Device(Guid id, string name, DeviceType type, bool isOn)
    {
        ValidateName(name);

        Id = id;
        Name = name;
        Type = type;
        IsOn = isOn;
    }

    public static Device Rehydrate(Guid id, string name, DeviceType type, bool isOn)
    {
        return new Device(id, name, type, isOn);
    }

    public static DeviceRegistered Register(string name, DeviceType type)
    {
        ValidateName(name);

        return new DeviceRegistered(Guid.NewGuid(), name, type);
    }

    public static Device Create(DeviceRegistered @event)
    {
        return new Device(@event.DeviceId, @event.Name, @event.Type, isOn: false);
    }

    public LightTurnedOn TurnOn()
    {
        if (Type != DeviceType.Light)
        {
            throw new DeviceCannotBeTurnedOnException(Id, Type);
        }

        if (IsOn)
        {
            throw new DeviceAlreadyOnException(Id);
        }

        var @event = new LightTurnedOn(Id);
        Apply(@event);

        return @event;
    }

    public void Apply(LightTurnedOn @event)
    {
        IsOn = true;
    }

    public LightTurnedOff TurnOff()
    {
        if (Type != DeviceType.Light)
        {
            throw new DeviceCannotBeTurnedOffException(Id, Type);
        }

        if (!IsOn)
        {
            throw new DeviceAlreadyOffException(Id);
        }

        var @event = new LightTurnedOff(Id);
        Apply(@event);

        return @event;
    }

    public void Apply(LightTurnedOff @event)
    {
        IsOn = false;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDeviceNameException();
        }
    }
}
