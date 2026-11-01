using System.Security.Cryptography;
using System.Text;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence.Seeders;

/// <summary>
/// Adds Development fixtures for user lists and auth history screens.
/// The reserved 202-555-01xx numbers are fictional; sessions and OTPs are already expired.
/// Active fixture accounts use the Development-only password 123456.
/// </summary>
public sealed class AuthDevelopmentSampleDataSeeder(
    IWebHostEnvironment environment,
    IPasswordHasher<UserEntity> passwordHasher,
    ILogger<AuthDevelopmentSampleDataSeeder> logger) : ISeeder
{
    private const int SampleUserCount = 24;
    private const string SamplePassword = "123456";

    public int Order => 4;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        if (!environment.IsDevelopment())
            return 0;

        var phones = Enumerable.Range(0, SampleUserCount)
            .Select(index => $"+1202555{100 + index:0000}")
            .ToArray();
        var existingUsers = await context.Users
            .Where(user => phones.Contains(user.Phone))
            .ToDictionaryAsync(user => user.Phone);

        var now = DateTimeOffset.UtcNow;
        var added = 0;
        var passwordsReset = 0;
        for (var index = 0; index < phones.Length; index++)
        {
            var phone = phones[index];
            if (existingUsers.TryGetValue(phone, out var existingUser))
            {
                var passwordStatus = passwordHasher.VerifyHashedPassword(
                    existingUser,
                    existingUser.Password,
                    SamplePassword);
                if (passwordStatus != PasswordVerificationResult.Success)
                {
                    existingUser.Password = passwordHasher.HashPassword(existingUser, SamplePassword);
                    passwordsReset++;
                }
                continue;
            }

            var createdAt = now.AddDays(-30 - index);
            var isActive = index < 16;
            var isPendingRegistration = index is >= 16 and < 20;
            var user = new UserEntity
            {
                PublicId = StableGuid($"development-sample-user:{phone}"),
                Phone = phone,
                IsActive = isActive,
                IsVerified = !isPendingRegistration,
                VerifiedAt = isPendingRegistration ? null : createdAt.AddMinutes(1),
                Role = UserRole.User,
                PermissionId = null,
                CreatedAt = createdAt,
                LastActiveAt = isPendingRegistration ? null : createdAt.AddHours(6 + index % 12)
            };

            user.Password = passwordHasher.HashPassword(user, SamplePassword);

            AddOtp(user, OtpPurpose.Registration, createdAt.AddMinutes(1), verified: !isPendingRegistration);

            if (!isPendingRegistration)
            {
                AddOtp(user, OtpPurpose.PasswordReset, createdAt.AddHours(3), verified: true);
                AddHistoricalSession(user, createdAt.AddHours(5), revoked: true, slot: 1);
                AddHistoricalSession(user, createdAt.AddDays(1), revoked: false, slot: 2);
            }

            context.Users.Add(user);
            added += 1 + user.AuthOtps.Count + user.AuthSessions.Count;
            existingUsers.Add(phone, user);
        }

        if (added > 0 || passwordsReset > 0)
            await context.SaveChangesAsync();

        logger.LogInformation(
            "Development auth sample data: added {AddedCount} record(s), reset {PasswordResetCount} fixture password(s).",
            added,
            passwordsReset);
        return added;

        void AddOtp(UserEntity user, OtpPurpose purpose, DateTimeOffset createdAt, bool verified)
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            user.AuthOtps.Add(new AuthOtpEntity
            {
                PublicId = StableGuid($"development-sample-otp:{user.Phone}:{purpose}"),
                Purpose = purpose,
                Code = passwordHasher.HashPassword(user, code),
                CreatedAt = createdAt,
                ExpiredAt = createdAt.AddMinutes(5),
                VerifiedAt = verified ? createdAt.AddMinutes(1) : null
            });
        }

        static void AddHistoricalSession(
            UserEntity user,
            DateTimeOffset createdAt,
            bool revoked,
            int slot)
        {
            var revokedAt = revoked ? createdAt.AddHours(1) : (DateTimeOffset?)null;
            var refreshTokenHash = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes($"development-sample-session:{user.Phone}:{slot}")));

            user.AuthSessions.Add(new AuthSessionEntity
            {
                RefreshToken = refreshTokenHash,
                CreatedAt = createdAt,
                ExpiredAt = createdAt.AddDays(14),
                RevokedAt = revokedAt
            });
        }
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
