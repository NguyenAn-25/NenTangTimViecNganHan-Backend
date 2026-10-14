namespace DotNetSkeleton.Shared.Configurations;
using DotNetSkeleton.Shared.Persistence.Seeders;

public static class SeederExtensions
{
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        // Tự động quét và đăng ký tất cả class implement ISeeder
        var seederTypes = typeof(ISeeder).Assembly
            .GetTypes()
            .Where(t => typeof(ISeeder).IsAssignableFrom(t)
                        && t is { IsClass: true, IsAbstract: false });

        foreach (var type in seederTypes)
        {
            services.AddScoped(typeof(ISeeder), type);
        }

        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.RunAsync();
    }
}