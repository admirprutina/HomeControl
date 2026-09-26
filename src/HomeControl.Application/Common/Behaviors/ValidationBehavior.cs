using FluentValidation;
using FluentValidation.Results;
using HomeControl.Application.Messaging;

namespace HomeControl.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await ValidationHelper.ValidateAsync(request, validators, cancellationToken);
        return await next();
    }
}

public sealed class ValidationBehavior<TRequest>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public async Task Handle(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        await ValidationHelper.ValidateAsync(request, validators, cancellationToken);
        await next();
    }
}

internal static class ValidationHelper
{
    internal static async Task ValidateAsync<TRequest>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
        {
            throw new HomeControl.Application.Common.Exceptions.ValidationException(failures);
        }
    }
}
