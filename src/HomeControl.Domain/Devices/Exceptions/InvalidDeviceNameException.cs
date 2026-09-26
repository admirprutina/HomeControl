namespace HomeControl.Domain.Devices.Exceptions;

public sealed class InvalidDeviceNameException : Exception
{
    public InvalidDeviceNameException()
        : base("Device name cannot be empty.")
    {
    }
}
