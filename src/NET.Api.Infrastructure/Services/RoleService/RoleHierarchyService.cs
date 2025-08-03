using Microsoft.AspNetCore.Identity;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services.RoleService;

/// <summary>
/// Implementación del servicio de dominio para jerarquías de roles
/// Responsabilidad única: Lógica pura de jerarquías de roles
/// </summary>
public class RoleHierarchyService(
    RoleManager<ApplicationRole> roleManager) : IRoleHierarchyService
{
    public async Task<int> GetRoleHierarchyLevelAsync(string roleName)
    {
        // Primero verificar si es un rol estático
        if (RoleConstants.IsStaticRole(roleName))
        {
            return RoleConstants.GetRoleHierarchy(roleName);
        }

        // Si es dinámico, obtener de la base de datos
        var role = await roleManager.FindByNameAsync(roleName);
        return role?.HierarchyLevel ?? RoleConstants.Hierarchy.User;
    }

    public async Task<string> GetHighestRoleAsync(IEnumerable<string> roles)
    {
        if (!roles.Any())
            return RoleConstants.Names.User;

        var highestRole = RoleConstants.Names.User;
        var highestHierarchy = 0;

        foreach (var role in roles)
        {
            var hierarchy = await GetRoleHierarchyLevelAsync(role);
            if (hierarchy > highestHierarchy)
            {
                highestHierarchy = hierarchy;
                highestRole = role;
            }
        }

        return highestRole;
    }

    public async Task<bool> IsHigherThanAsync(string role1, string role2)
    {
        var hierarchy1 = await GetRoleHierarchyLevelAsync(role1);
        var hierarchy2 = await GetRoleHierarchyLevelAsync(role2);
        return hierarchy1 > hierarchy2;
    }

    public async Task<bool> IsEqualOrHigherThanAsync(string role1, string role2)
    {
        var hierarchy1 = await GetRoleHierarchyLevelAsync(role1);
        var hierarchy2 = await GetRoleHierarchyLevelAsync(role2);
        return hierarchy1 >= hierarchy2;
    }

    public async Task<IEnumerable<string>> GetSubordinateRolesAsync(string roleName)
    {
        var roleHierarchy = await GetRoleHierarchyLevelAsync(roleName);
        var subordinateRoles = new List<string>();

        // Agregar roles estáticos subordinados
        foreach (var staticRole in RoleConstants.AllRoles)
        {
            var staticHierarchy = RoleConstants.GetRoleHierarchy(staticRole);
            if (staticHierarchy < roleHierarchy)
            {
                subordinateRoles.Add(staticRole);
            }
        }

        // Agregar roles dinámicos subordinados
        var dynamicRoles = roleManager.Roles.Where(r => r.IsActive && r.HierarchyLevel < roleHierarchy);
        foreach (var role in dynamicRoles)
        {
            if (role.Name != null && !subordinateRoles.Contains(role.Name))
            {
                subordinateRoles.Add(role.Name);
            }
        }

        return subordinateRoles;
    }

    public async Task<bool> IsValidRoleAsync(string roleName)
    {
        // Verificar si es un rol estático
        if (RoleConstants.IsStaticRole(roleName))
        {
            return true;
        }

        // Verificar si es un rol dinámico activo
        var role = await roleManager.FindByNameAsync(roleName);
        return role != null && role.IsActive;
    }
}