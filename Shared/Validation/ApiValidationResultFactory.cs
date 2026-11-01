using DotNetSkeleton.Shared.Api;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared;
using FluentValidation.Results;

namespace DotNetSkeleton.Shared.Validation;

public sealed class ApiValidationResultFactory : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(
        ActionExecutingContext context,
        ValidationProblemDetails validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        var response = ApiResponse<object?>.Fail(
            "Dữ liệu đầu vào không hợp lệ.",
            "validation_error",
            context.HttpContext.TraceIdentifier,
            validationProblemDetails.Errors.ToDictionary(entry => entry.Key, entry => entry.Value));

        return Task.FromResult<IActionResult?>(new BadRequestObjectResult(response));
    }
}
