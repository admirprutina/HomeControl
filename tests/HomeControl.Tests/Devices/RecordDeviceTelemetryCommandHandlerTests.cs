using HomeControl.Application;
using HomeControl.Application.Abstractions.Messaging;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Commands.RecordDeviceTelemetry;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using HomeControl.Application.Messaging;
using HomeControl.Domain.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControl.Tests.Devices;

public sealed class RecordDeviceTelemetryCommandHandlerTests
{
    [Fact]
    public async Task Missing_device_throws_without_publishing()
    {
        var id = Guid.NewGuid();
        var publisher = new RecordingPublisher();
        var handler = new RecordDeviceTelemetryCommandHandler(new StubRepository(null), publisher);

        var exception = await Assert.ThrowsAsync<DeviceNotFoundException>(() =>
            handler.Handle(new(id, 22.6m, 8.3m), CancellationToken.None));

        Assert.Equal(id, exception.DeviceId);
        Assert.Empty(publisher.Messages);
    }

    [Fact]
    public async Task Existing_device_publishes_exactly_one_message_with_values_and_utc_timestamp()
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Desk light", DeviceType.Light, false);
        var repository = new StubRepository(device);
        var publisher = new RecordingPublisher();
        var occurredAt = new DateTimeOffset(2026, 9, 28, 12, 30, 0, TimeSpan.FromHours(2));
        using var cancellation = new CancellationTokenSource();
        var handler = new RecordDeviceTelemetryCommandHandler(repository, publisher);

        var result = await handler.Handle(new(device.Id, 22.6m, 8.3m, occurredAt), cancellation.Token);

        var message = Assert.Single(publisher.Messages);
        Assert.Equal(device.Id, repository.RequestedId);
        Assert.Equal(device.Id, message.DeviceId);
        Assert.Equal(22.6m, message.TemperatureCelsius);
        Assert.Equal(8.3m, message.PowerUsageWatts);
        Assert.Equal(occurredAt.ToUniversalTime(), message.OccurredAtUtc);
        Assert.Equal(TimeSpan.Zero, message.OccurredAtUtc.Offset);
        Assert.Equal(new RecordDeviceTelemetryCommandResult(device.Id, message.OccurredAtUtc), result);
        Assert.Equal(cancellation.Token, repository.Token);
        Assert.Equal(cancellation.Token, publisher.Token);
        Assert.False(device.IsOn);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Either_measurement_alone_is_preserved_and_missing_timestamp_uses_utc_now(bool temperatureOnly)
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Desk light", DeviceType.Light, false);
        var publisher = new RecordingPublisher();
        var handler = new RecordDeviceTelemetryCommandHandler(new StubRepository(device), publisher);
        decimal? temperature = temperatureOnly ? 22.6m : null;
        decimal? power = temperatureOnly ? null : 8.3m;
        var before = DateTimeOffset.UtcNow;

        await handler.Handle(new(device.Id, temperature, power), CancellationToken.None);

        var message = Assert.Single(publisher.Messages);
        Assert.Equal(temperature, message.TemperatureCelsius);
        Assert.Equal(power, message.PowerUsageWatts);
        Assert.InRange(message.OccurredAtUtc, before, DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Publisher_failure_propagates_without_returning_success()
    {
        var device = Device.Rehydrate(Guid.NewGuid(), "Desk light", DeviceType.Light, false);
        var publisher = new RecordingPublisher { FailPublish = true };
        var handler = new RecordDeviceTelemetryCommandHandler(new StubRepository(device), publisher);

        await Assert.ThrowsAsync<PublishFailedException>(() =>
            handler.Handle(new(device.Id, 22.6m, null), CancellationToken.None));
        Assert.Single(publisher.Messages);
    }

    [Fact]
    public async Task Mediator_rejects_empty_measurements_before_repository_or_publisher_is_called()
    {
        var repository = new StubRepository(null);
        var publisher = new RecordingPublisher();
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IDeviceRepository>(repository);
        services.AddSingleton<IDeviceTelemetryPublisher>(publisher);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        await Assert.ThrowsAsync<HomeControl.Application.Common.Exceptions.ValidationException>(() =>
            scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new RecordDeviceTelemetryCommandRequest(Guid.NewGuid(), null, null), CancellationToken.None));

        Assert.Null(repository.RequestedId);
        Assert.Empty(publisher.Messages);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Validator_accepts_supplied_measurements_without_inventing_ranges(bool temperature, bool power)
    {
        var validator = new RecordDeviceTelemetryCommandRequestValidator();
        var result = validator.Validate(new RecordDeviceTelemetryCommandRequest(
            Guid.NewGuid(), temperature ? -1000m : null, power ? -1000m : null));

        Assert.True(result.IsValid);
    }

    private sealed class StubRepository(Device? device) : IDeviceRepository
    {
        public Guid? RequestedId { get; private set; }
        public CancellationToken Token { get; private set; }

        public Task<Device?> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken)
        {
            RequestedId = deviceId;
            Token = cancellationToken;
            return Task.FromResult(device);
        }
    }

    private sealed class RecordingPublisher : IDeviceTelemetryPublisher
    {
        public List<DeviceTelemetryRecorded> Messages { get; } = [];
        public CancellationToken Token { get; private set; }
        public bool FailPublish { get; init; }

        public Task PublishAsync(DeviceTelemetryRecorded message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            Token = cancellationToken;
            return FailPublish ? Task.FromException(new PublishFailedException()) : Task.CompletedTask;
        }
    }

    private sealed class PublishFailedException : Exception { }
}
