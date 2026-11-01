using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Constants;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence.Seeders;

public sealed class AuthPermissionCatalogSeeder : ISeeder
{
    private static readonly (string NameVn, string NameEn, string[] Actions)[] FunctionCatalog =
    [
        // Keep this catalog aligned with the permission policies on implemented endpoints.
        ("Quản lý người dùng", "users", ["read", "create", "update"])
    ];

    private static readonly (string Function, string Action)[] InitialAdminGrants =
    [
        ("users", "read"),
        ("users", "create"),
        ("users", "update")
    ];

    private static readonly string[] RetiredFunctionNames = ["auth", "permissions"];

    public int Order => 1;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        var added = 0;
        var functions = new Dictionary<string, FunctionEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in FunctionCatalog)
        {
            var function = await context.Functions
                .SingleOrDefaultAsync(x => x.NameEn == definition.NameEn);
            if (function is null)
            {
                function = new FunctionEntity
                {
                    NameVn = definition.NameVn,
                    NameEn = definition.NameEn,
                    Actions = definition.Actions
                };
                context.Functions.Add(function);
                added++;
            }
            else
            {
                function.NameVn = definition.NameVn;
                function.Actions = definition.Actions;
            }

            functions.Add(definition.NameEn, function);
        }

        var adminPermissionWasCreated = !await context.Permissions
            .AnyAsync(x => x.Name == PermissionConstants.DefaultAdminProfileName);
        var adminPermission = await GetOrCreatePermissionAsync(
            context,
            PermissionConstants.DefaultAdminProfileName,
            "Permission profile mặc định cho user có role ADMIN.",
            () => added++);
        await context.SaveChangesAsync();

        // Remove retired functions and unsupported actions while preserving grants
        // from every permission profile, including custom profiles assigned to ADMIN users.
        var obsoleteGrants = await context.PermissionDetails
            .Include(detail => detail.Permission)
            .Include(detail => detail.Function)
            .ToListAsync();
        var supportedActions = FunctionCatalog.ToDictionary(
            definition => definition.NameEn,
            definition => definition.Actions,
            StringComparer.OrdinalIgnoreCase);
        context.PermissionDetails.RemoveRange(obsoleteGrants.Where(detail =>
            RetiredFunctionNames.Any(name =>
                string.Equals(name, detail.Function.NameEn, StringComparison.OrdinalIgnoreCase)) ||
            (supportedActions.TryGetValue(detail.Function.NameEn, out var actions) &&
             !actions.Any(action => string.Equals(
                 action,
                 detail.Action,
                 StringComparison.OrdinalIgnoreCase)))));

        var retiredFunctions = await context.Functions
            .Where(function => RetiredFunctionNames.Contains(function.NameEn))
            .ToListAsync();
        context.Functions.RemoveRange(retiredFunctions);

        // Seed a small initial admin grant set only when the role is first created.
        // On later startups, intentional permission_details edits (including removals) persist.
        if (adminPermissionWasCreated)
        {
            foreach (var grant in InitialAdminGrants)
                added += await AddGrantIfMissingAsync(
                    context,
                    adminPermission,
                    functions[grant.Function],
                    grant.Action);
        }

        await context.SaveChangesAsync();
        return added;
    }

    private static async Task<PermissionEntity> GetOrCreatePermissionAsync(
        AppDbContext context,
        string name,
        string description,
        Action onAdded)
    {
        var permission = await context.Permissions.SingleOrDefaultAsync(x => x.Name == name);
        if (permission is not null)
        {
            permission.Description = description;
            return permission;
        }

        permission = new PermissionEntity { Name = name, Status = PermissionStatus.Active, Description = description };
        context.Permissions.Add(permission);
        onAdded();
        return permission;
    }

    private static async Task<int> AddGrantIfMissingAsync(
        AppDbContext context,
        PermissionEntity permission,
        FunctionEntity function,
        string action)
    {
        var exists = await context.PermissionDetails.AnyAsync(x =>
            x.PermissionId == permission.Id && x.FunctionId == function.Id && x.Action == action);
        if (exists)
            return 0;

        context.PermissionDetails.Add(new PermissionDetailEntity
        {
            Permission = permission,
            Function = function,
            Action = action
        });
        return 1;
    }
}
