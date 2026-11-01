namespace DotNetSkeleton.Shared.Configurations;
using DotNetSkeleton.Shared.Persistence.Seeders;

public static class SeederExtensions
{
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        // This auth flow seeds only the six auth/permission entities in scope.
        services.AddScoped<ISeeder, AuthPermissionCatalogSeeder>();
        services.AddScoped<ISeeder, AuthDevelopmentAdminSeeder>();
        services.AddScoped<ISeeder, AuthDevelopmentAdminAccountsSeeder>();
        services.AddScoped<ISeeder, AuthDevelopmentSampleDataSeeder>();
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
