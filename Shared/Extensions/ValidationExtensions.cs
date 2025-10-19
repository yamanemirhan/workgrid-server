using FluentValidation.Results;
using Shared.Responses;

namespace Shared.Extensions;

public static class ValidationExtensions
{
    public static ApiResponse ToApiResponse(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ResponseHelper.ValidationError(errors);
    }

    public static ApiResponse<T> ToApiResponse<T>(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ResponseHelper.Error<T>("Validation failed", errors);
    }
}