using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Mapster;

namespace DotNetSkeleton.Modules.AuthModule.Mappings;

public static class AuthMappingConfiguration
{
    public static TypeAdapterConfig Config { get; } = CreateConfig();

    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<UserEntity, UserResponseDto>()
            .Map(destination => destination.Id, source => source.Id)
            .Map(destination => destination.PublicId, source => source.PublicId)
            .Map(destination => destination.Phone, source => source.Phone)
            .Map(destination => destination.IsActive, source => source.IsActive)
            .Map(destination => destination.IsVerified, source => source.IsVerified)
            .Map(destination => destination.VerifiedAt, source => source.VerifiedAt)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt)
            .Map(destination => destination.Role, source => source.Role);

        config.NewConfig<UserEntity, UserSummaryResponseDto>()
            .Map(destination => destination.Id, source => source.Id)
            .Map(destination => destination.PublicId, source => source.PublicId)
            .Map(destination => destination.Phone, source => source.Phone)
            .Map(destination => destination.IsActive, source => source.IsActive)
            .Map(destination => destination.IsVerified, source => source.IsVerified)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt)
            .Map(destination => destination.Role, source => source.Role)
            .Map(
                destination => destination.Permission,
                source => source.Permission == null
                    ? null
                    : new PermissionSummaryResponseDto(
                        source.Permission.Id,
                        source.Permission.Name));

        config.NewConfig<UserEntity, UserDetailResponseDto>()
            .Map(destination => destination.Id, source => source.Id)
            .Map(destination => destination.PublicId, source => source.PublicId)
            .Map(destination => destination.Phone, source => source.Phone)
            .Map(destination => destination.IsActive, source => source.IsActive)
            .Map(destination => destination.IsVerified, source => source.IsVerified)
            .Map(destination => destination.VerifiedAt, source => source.VerifiedAt)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt)
            .Map(destination => destination.LastActiveAt, source => source.LastActiveAt)
            .Map(destination => destination.Role, source => source.Role)
            .Map(
                destination => destination.Permission,
                source => source.Permission == null
                    ? null
                    : new PermissionDetailResponseDto(
                        source.Permission.Id,
                        source.Permission.Name,
                        source.Permission.Description,
                        source.Permission.Status));

        return config;
    }
}
