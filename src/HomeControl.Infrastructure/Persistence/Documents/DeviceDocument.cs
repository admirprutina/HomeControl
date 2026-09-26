using HomeControl.Domain.Devices;

namespace HomeControl.Infrastructure.Persistence.Documents;

internal sealed class DeviceDocument
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DeviceType Type { get; set; }

    public bool IsOn { get; set; }
}
