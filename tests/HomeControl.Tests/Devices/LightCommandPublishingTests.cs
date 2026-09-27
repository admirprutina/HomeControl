using HomeControl.Application.Abstractions.Messaging;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Commands.TurnOffLight;
using HomeControl.Application.Features.Devices.Commands.TurnOnLight;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using HomeControl.Domain.Devices;
using HomeControl.Domain.Devices.Events;

namespace HomeControl.Tests.Devices;

public sealed class LightCommandPublishingTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Publishes_new_state_after_event_stream_is_saved(bool turnOn)
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Desk light", DeviceType.Light, !turnOn);
        var stream = new StubEventStream(device);
        var publisher = new RecordingPublisher(stream);
        var store = new StubEventStore(stream);

        if (turnOn)
        {
            await new TurnOnLightCommandHandler(store, publisher)
                .Handle(new TurnOnLightCommandRequest(device.Id), CancellationToken.None);
        }
        else
        {
            await new TurnOffLightCommandHandler(store, publisher)
                .Handle(new TurnOffLightCommandRequest(device.Id), CancellationToken.None);
        }

        Assert.True(stream.Saved);
        Assert.True(publisher.WasSavedWhenPublished);
        Assert.NotNull(publisher.Message);
        Assert.Equal(device.Id, publisher.Message.DeviceId);
        Assert.Equal(turnOn, publisher.Message.IsOn);
        Assert.True(publisher.Message.OccurredAtUtc > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Does_not_publish_when_save_fails(bool turnOn)
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Desk light", DeviceType.Light, !turnOn);
        var stream = new StubEventStream(device) { FailSave = true };
        var publisher = new RecordingPublisher(stream);
        var store = new StubEventStore(stream);

        if (turnOn)
        {
            await Assert.ThrowsAsync<SaveFailedException>(() =>
                new TurnOnLightCommandHandler(store, publisher)
                    .Handle(new TurnOnLightCommandRequest(device.Id), CancellationToken.None));
        }
        else
        {
            await Assert.ThrowsAsync<SaveFailedException>(() =>
                new TurnOffLightCommandHandler(store, publisher)
                    .Handle(new TurnOffLightCommandRequest(device.Id), CancellationToken.None));
        }

        Assert.Null(publisher.Message);
    }

    private sealed class StubEventStore(IDeviceEventStream stream) : IDeviceEventStore
    {
        public Task StartAsync(DeviceRegistered @event,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IDeviceEventStream?> FetchForWritingAsync(Guid deviceId,
            CancellationToken cancellationToken) => Task.FromResult<IDeviceEventStream?>(stream);
    }

    private sealed class StubEventStream(Device aggregate) : IDeviceEventStream
    {
        public Device Aggregate { get; } = aggregate;
        public bool Saved { get; private set; }
        public bool FailSave { get; init; }

        public void Append(object @event) { }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (FailSave)
            {
                throw new SaveFailedException();
            }

            Saved = true;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPublisher(StubEventStream stream) : IDeviceEventPublisher
    {
        public DeviceLightStateChanged? Message { get; private set; }
        public bool WasSavedWhenPublished { get; private set; }

        public Task PublishAsync(DeviceLightStateChanged message, CancellationToken cancellationToken)
        {
            Message = message;
            WasSavedWhenPublished = stream.Saved;
            return Task.CompletedTask;
        }
    }

    private sealed class SaveFailedException : Exception { }
}
