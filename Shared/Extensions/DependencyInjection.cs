using DotNetSkeleton.Modules.ExampleModule;
using DotNetSkeleton.Modules.ExampleModule.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace DotNetSkeleton.Shared.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký các Service / Repository
        services.AddScoped<IExampleService, ExampleService>();

        // Tương lai có thêm Service khác thì đăng ký thêm ở đây:
        // services.AddScoped<IUserService, UserService>();


        // Đăng ký toàn bộ Validator trong Assembly hiện tại (tự động scan toàn bộ validator)
        services.AddValidatorsFromAssemblyContaining<CreateExampleDtoValidator>();

        // Cấu hình tự động validate
        services.AddFluentValidationAutoValidation();

        return services;
    }
}