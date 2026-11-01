using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Constants;
using DotNetSkeleton.Modules.AuthModule.Enums;
using DotNetSkeleton.Shared.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence.Seeders;

/// <summary>
/// Creates one local development administrator when explicit User Secrets are supplied.
/// This seeder never runs outside Development and never overwrites an existing account.
/// </summary>
public sealed class AuthDevelopmentAdminSeeder(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IPasswordHasher<UserEntity> passwordHasher,
    ILogger<AuthDevelopmentAdminSeeder> logger) : ISeeder
{
    public int Order => 2;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        if (!environment.IsDevelopment())
            return 0;

        var phone = configuration["Seed:AdminPhone"]?.Trim();
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation(
                "Development admin seed skipped; configure Seed:AdminPhone and Seed:AdminPassword to create it.");
            return 0;
        }

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Development admin seed requires both Seed:AdminPhone and Seed:AdminPassword.");
        }

        if (!CommonValidationRules.IsE164Phone(phone))
        {
            throw new InvalidOperationException(
                "Seed:AdminPhone must use E.164 format, for example +84901234567.");
        }

        if (!CommonValidationRules.IsRequiredText(password) ||
            !CommonValidationRules.IsPasswordStrengthValid(password))
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword must be 8-128 characters and include an uppercase letter, a digit, and a special character.");
        }

        var adminPermission = await context.Permissions.SingleOrDefaultAsync(
            permission => permission.Name == PermissionConstants.DefaultAdminProfileName &&
                          permission.Status == PermissionStatus.Active);
        if (adminPermission is null)
        {
            throw new InvalidOperationException(
                "The active admin permission is missing. Run AuthPermissionCatalogSeeder before AuthDevelopmentAdminSeeder.");
        }

        var existingUser = await context.Users.SingleOrDefaultAsync(user => user.Phone == phone);
        if (existingUser is not null)
        {
            var isConfiguredAdmin = existingUser.IsActive &&
                                    existingUser.IsVerified &&
                                    existingUser.VerifiedAt is not null &&
                                    existingUser.Role == UserRole.Admin &&
                                    existingUser.PermissionId == adminPermission.Id;
            var passwordMatches = passwordHasher.VerifyHashedPassword(
                existingUser,
                existingUser.Password,
                password) != PasswordVerificationResult.Failed;

            if (isConfiguredAdmin && passwordMatches)
                return 0;

            throw new InvalidOperationException(
                "Seed:AdminPhone already belongs to a different or differently configured user. " +
                "The seeder will not modify existing accounts; choose another phone or bootstrap the account explicitly.");
        }

        var user = new UserEntity
        {
            Phone = phone,
            IsActive = true,
            IsVerified = true,
            VerifiedAt = DateTimeOffset.UtcNow,
            Role = UserRole.Admin,
            PermissionId = adminPermission.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.Password = passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return 1;
    }
}
