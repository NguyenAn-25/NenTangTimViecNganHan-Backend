using DotNetSkeleton.Modules.ExampleModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Converters;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<Example> Examples { get; set; }
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AuthSessionEntity> AuthSessions => Set<AuthSessionEntity>();
    public DbSet<AuthOtpEntity> AuthOtps => Set<AuthOtpEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<PermissionDetailEntity> PermissionDetails => Set<PermissionDetailEntity>();
    public DbSet<FunctionEntity> Functions => Set<FunctionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>()
            .HasOne(user => user.Permission)
            .WithMany(permission => permission.Users)
            .HasForeignKey(user => user.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserEntity>()
            .Property(user => user.Role)
            .HasConversion(new UserRoleValueConverter())
            .HasMaxLength(16);

        modelBuilder.Entity<UserEntity>()
            .Property(user => user.IsVerified)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserEntity>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_user_role_permission",
                "(role = 'USER' AND permission_id IS NULL) OR (role = 'ADMIN' AND permission_id IS NOT NULL)"));

        modelBuilder.Entity<UserEntity>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_user_verification_state",
                "(is_verified = TRUE AND verified_at IS NOT NULL) OR (is_verified = FALSE AND verified_at IS NULL)"));

        modelBuilder.Entity<AuthOtpEntity>()
            .Property(otp => otp.Purpose)
            .HasConversion(new OtpPurposeValueConverter())
            .HasMaxLength(32);

        modelBuilder.Entity<PermissionEntity>()
            .Property(permission => permission.Status)
            .HasConversion(new PermissionStatusValueConverter())
            .HasMaxLength(24);

    }
}
