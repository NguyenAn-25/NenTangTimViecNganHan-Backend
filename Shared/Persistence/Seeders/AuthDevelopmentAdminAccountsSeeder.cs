using System.Security.Cryptography;
using System.Text;
using DotNetSkeleton.Modules.AuthModule.Constants;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence.Seeders;

/// <summary>
/// Creates fictional administrator accounts for local Development use only.
/// This seeder never runs outside Development and never promotes existing accounts.
/// </summary>
public sealed class AuthDevelopmentAdminAccountsSeeder(
    IWebHostEnvironment environment,
    IPasswordHasher<UserEntity> passwordHasher,
    ILogger<AuthDevelopmentAdminAccountsSeeder> logger) : ISeeder
{
    private const string DevelopmentPassword = "123456";
    private const string PreviousDevelopmentPassword = "Admin@123456";

    private static readonly string[] AdminPhones =
    [
        "+12025550190",
        "+12025550191"
    ];

    public int Order => 3;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        if (!environment.IsDevelopment())
            return 0;

        var permission = await context.Permissions.SingleOrDefaultAsync(
            profile => profile.Name == PermissionConstants.DefaultAdminProfileName &&
                       profile.Status == PermissionStatus.Active);
        if (permission is null)
        {
            throw new InvalidOperationException(
                "The active admin permission is missing. Run AuthPermissionCatalogSeeder before AuthDevelopmentAdminAccountsSeeder.");
        }

        var existingUsers = await context.Users
            .Where(user => AdminPhones.Contains(user.Phone))
            .ToDictionaryAsync(user => user.Phone);

        var added = 0;
        var passwordsUpdated = 0;
        foreach (var phone in AdminPhones)
        {
            if (existingUsers.TryGetValue(phone, out var existingUser))
            {
                var isConfiguredAdmin = existingUser.IsActive &&
                                        existingUser.IsVerified &&
                                        existingUser.VerifiedAt is not null &&
                                        existingUser.Role == UserRole.Admin &&
                                        existingUser.PermissionId == permission.Id;
                var passwordMatches = passwordHasher.VerifyHashedPassword(
                    existingUser,
                    existingUser.Password,
                    DevelopmentPassword) != PasswordVerificationResult.Failed;

                if (isConfiguredAdmin && passwordMatches)
                    continue;

                var hasPreviousFixturePassword = passwordHasher.VerifyHashedPassword(
                    existingUser,
                    existingUser.Password,
                    PreviousDevelopmentPassword) != PasswordVerificationResult.Failed;
                if (isConfiguredAdmin && hasPreviousFixturePassword)
                {
                    existingUser.Password = passwordHasher.HashPassword(existingUser, DevelopmentPassword);
                    passwordsUpdated++;
                    continue;
                }

                throw new InvalidOperationException(
                    $"Development admin fixture phone {phone} already belongs to a different or differently configured user. " +
                    "The seeder will not modify or promote existing accounts.");
            }

            var user = new UserEntity
            {
                PublicId = StableGuid($"development-admin:{phone}"),
                Phone = phone,
                Password = string.Empty,
                IsActive = true,
                IsVerified = true,
                VerifiedAt = DateTimeOffset.UtcNow,
                Role = UserRole.Admin,
                PermissionId = permission.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.Password = passwordHasher.HashPassword(user, DevelopmentPassword);

            context.Users.Add(user);
            added++;
        }

        if (added > 0 || passwordsUpdated > 0)
            await context.SaveChangesAsync();

        logger.LogInformation(
            "Development admin fixtures: added {AddedCount} account(s), updated {PasswordUpdatedCount} fixture password(s).",
            added,
            passwordsUpdated);
        return added;
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
