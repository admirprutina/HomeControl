using FluentValidation;

namespace HomeControl.Application.Features.Devices.Commands.RecordDeviceTelemetry;

public sealed class RecordDeviceTelemetryCommandRequestValidator
    : AbstractValidator<RecordDeviceTelemetryCommandRequest>
{
    public RecordDeviceTelemetryCommandRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.TemperatureCelsius is not null || request.PowerUsageWatts is not null)
            .WithMessage("At least one telemetry measurement is required.");
    }
}
