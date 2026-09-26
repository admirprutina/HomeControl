using FluentValidation;

namespace HomeControl.Application.Features.Devices.Commands.RegisterDevice;

public sealed class RegisterDeviceCommandRequestValidator : AbstractValidator<RegisterDeviceCommandRequest>
{
    public RegisterDeviceCommandRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Device name is required.")
            .MaximumLength(100).WithMessage("Device name must be 100 characters or fewer.");

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage("Device type must be a defined device type.");
    }
}
