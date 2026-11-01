using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Shared.Exceptions;
using DotNetSkeleton.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace DotNetSkeleton.Shared.Extensions;

public static class SharedApiExtensions
{
    public static IServiceCollection AddSharedApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddFluentValidationAutoValidation(configuration =>
            configuration.OverrideDefaultResultFactoryWith<ApiValidationResultFactory>());
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "Giá trị không hợp lệ."
                                : error.ErrorMessage)
                            .Distinct()
                            .ToArray());

                var response = ApiResponse<object?>.Fail(
                    "Dữ liệu đầu vào không hợp lệ.",
                    "validation_error",
                    context.HttpContext.TraceIdentifier,
                    errors);
                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
