using HomeControl.Application.Common.Exceptions;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Domain.Devices.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Api.Common.ExceptionHandling;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            ValidationException validationException => new ValidationProblemDetails(
                validationException.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            },
            DeviceNotFoundException deviceNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Device not found",
                Detail = deviceNotFoundException.Message
            },
            DeviceAlreadyOnException deviceAlreadyOnException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Device already on",
                Detail = deviceAlreadyOnException.Message
            },
            DeviceAlreadyOffException deviceAlreadyOffException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Device already off",
                Detail = deviceAlreadyOffException.Message
            },
            DeviceCannotBeTurnedOnException deviceCannotBeTurnedOnException => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Device cannot be turned on",
                Detail = deviceCannotBeTurnedOnException.Message
            },
            DeviceCannotBeTurnedOffException deviceCannotBeTurnedOffException => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Device cannot be turned off",
                Detail = deviceCannotBeTurnedOffException.Message
            },
            DeviceConcurrencyException deviceConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Device state changed",
                Detail = deviceConcurrencyException.Message
            },
            InvalidDeviceNameException invalidDeviceNameException => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Invalid device",
                Detail = invalidDeviceNameException.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "An error occurred while processing the request."
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
