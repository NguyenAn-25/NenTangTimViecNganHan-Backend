using Microsoft.AspNetCore.Authorization;
using DotNetSkeleton.Modules.AuthModule.Constants;

namespace DotNetSkeleton.Modules.AuthModule.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var expected = $"{requirement.Function}:{requirement.Action}";
        if (context.User.HasClaim(PermissionConstants.PermissionClaim, expected))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
