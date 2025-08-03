using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NET.Api.Application.Abstractions.Services.IRoleService;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services.RoleService;

/// <summary>
/// Implementación del servicio de aplicación para consultas de roles
/// Responsabilidad única: Consultas y operaciones de lectura de roles
/// </summary>
public class RoleQueryService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IRoleAuthorizationService roleAuthorizationService) : IRoleQueryService
{
    public async Task<IEnumerable<ApplicationRole>> GetAllRolesAsync()
    {
        return await roleManager.Roles.ToListAsync();
    }

    public async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
    {
        return await roleManager.FindByIdAsync(roleId);
    }

    public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
    {
        return await roleManager.FindByNameAsync(roleName);
    }

    public async Task<IEnumerable<string>> GetAssignableRolesAsync(IEnumerable<string> userRoles)
    {
        return await roleAuthorizationService.GetAssignableRolesAsync(userRoles);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName)
    {
        return await userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Enumerable.Empty<string>();

        return await userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<ApplicationRole>> GetActiveRolesAsync()
    {
        return await roleManager.Roles.ToListAsync();
    }

    public async Task<IEnumerable<ApplicationRole>> GetSystemRolesAsync()
    {
        return await roleManager.Roles
            .Where(r => r.IsSystemRole)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationRole>> GetCustomRolesAsync()
    {
        return await roleManager.Roles
            .Where(r => !r.IsSystemRole)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesByHierarchyLevelAsync(int hierarchyLevel)
    {
        return await roleManager.Roles
            .Where(r => r.HierarchyLevel == hierarchyLevel)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesWithHierarchyLevelOrLowerAsync(int maxHierarchyLevel)
    {
        return await roleManager.Roles
            .Where(r => r.HierarchyLevel <= maxHierarchyLevel)
            .OrderBy(r => r.HierarchyLevel)
            .ToListAsync();
    }

    public async Task<int> GetRoleCountAsync()
    {
        return await roleManager.Roles.CountAsync();
    }

    public async Task<int> GetActiveRoleCountAsync()
    {
        return await roleManager.Roles.CountAsync();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersWithMultipleRolesAsync()
    {
        var allUsers = await userManager.Users.ToListAsync();
        var usersWithMultipleRoles = new List<ApplicationUser>();

        foreach (var user in allUsers)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count > 1)
            {
                usersWithMultipleRoles.Add(user);
            }
        }

        return usersWithMultipleRoles;
    }

    public async Task<Dictionary<string, int>> GetRoleUserCountsAsync()
    {
        var roles = await roleManager.Roles.ToListAsync();
        var roleUserCounts = new Dictionary<string, int>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            roleUserCounts[role.Name!] = usersInRole.Count;
        }

        return roleUserCounts;
    }
}