using FluentValidation;
using ProjectManagement.Application.Common.Exceptions;

namespace ProjectManagement.Application.Common.Extensions;

public static class ValidationExtensions
{

    public static async Task ValidateAndThrowAppAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);

        var validationResult =
            await validator.ValidateAsync(
                instance,
                cancellationToken);

        if (validationResult.IsValid)
        {
            return;
        }

        var errors =
            validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .Distinct()
                        .ToArray());

        throw new RequestValidationException(
            "Gönderilen bilgiler geçerli değildir.",
            errors);
    }
}