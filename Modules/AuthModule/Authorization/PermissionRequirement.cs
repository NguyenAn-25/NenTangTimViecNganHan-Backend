using Microsoft.AspNetCore.Authorization;

namespace DotNetSkeleton.Modules.AuthModule.Authorization;

public sealed record PermissionRequirement(string Function, string Action) : IAuthorizationRequirement;
