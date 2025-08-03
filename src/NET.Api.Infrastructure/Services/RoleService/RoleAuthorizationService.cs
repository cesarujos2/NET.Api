using NET.Api.Domain.Services;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services.RoleService;

/// <summary>
/// Implementación del servicio de dominio para autorización de roles
/// Responsabilidad única: Lógica de autorización y permisos de roles
/// </summary>
public class RoleAuthorizationService(
    IRoleHierarchyService roleHierarchyService) : IRoleAuthorizationService
{
    public async Task<bool> CanAssignRoleAsync(IEnumerable<string> userRoles, string targetRole)
    {
        // Verificar que el rol objetivo sea válido
        if (!await roleHierarchyService.IsValidRoleAsync(targetRole))
            return false;

        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        
        // El Owner puede asignar cualquier rol
        if (highestUserRole == RoleConstants.Names.Owner)
            return true;

        // Los demás solo pueden asignar roles de menor jerarquía
        return await roleHierarchyService.IsHigherThanAsync(highestUserRole, targetRole);
    }

    public async Task<bool> CanRemoveRoleAsync(IEnumerable<string> userRoles, string targetRole)
    {
        // Verificar que el rol objetivo sea válido
        if (!await roleHierarchyService.IsValidRoleAsync(targetRole))
            return false;

        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        
        // El Owner puede remover cualquier rol excepto Owner
        if (highestUserRole == RoleConstants.Names.Owner)
            return targetRole != RoleConstants.Names.Owner;

        // Los demás solo pueden remover roles de menor jerarquía
        return await roleHierarchyService.IsHigherThanAsync(highestUserRole, targetRole);
    }

    public async Task<bool> HasSufficientAuthorityAsync(IEnumerable<string> userRoles, string requiredRole)
    {
        // Verificar que el rol requerido sea válido
        if (!await roleHierarchyService.IsValidRoleAsync(requiredRole))
            return false;

        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        
        // El usuario debe tener jerarquía igual o mayor que la requerida
        return await roleHierarchyService.IsEqualOrHigherThanAsync(highestUserRole, requiredRole);
    }

    public async Task<bool> CanCreateRoleWithHierarchyAsync(IEnumerable<string> userRoles, int targetHierarchyLevel)
    {
        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        var userHierarchyLevel = await roleHierarchyService.GetRoleHierarchyLevelAsync(highestUserRole);
        
        // Solo se pueden crear roles con jerarquía menor que la del usuario
        return userHierarchyLevel > targetHierarchyLevel;
    }

    public async Task<bool> CanUpdateRoleWithHierarchyAsync(IEnumerable<string> userRoles, int currentRoleHierarchy, int newHierarchyLevel)
    {
        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        var userHierarchyLevel = await roleHierarchyService.GetRoleHierarchyLevelAsync(highestUserRole);
        
        // El usuario debe tener jerarquía mayor que el rol actual y el nuevo nivel
        return userHierarchyLevel > currentRoleHierarchy && userHierarchyLevel > newHierarchyLevel;
    }

    public async Task<IEnumerable<string>> GetAssignableRolesAsync(IEnumerable<string> userRoles)
    {
        var highestUserRole = await roleHierarchyService.GetHighestRoleAsync(userRoles);
        
        // Owner puede asignar todos los roles
        if (highestUserRole == RoleConstants.Names.Owner)
        {
            return RoleConstants.AllRoles;
        }

        // Los demás pueden asignar roles subordinados
        return await roleHierarchyService.GetSubordinateRolesAsync(highestUserRole);
    }

    public async Task<bool> CanManageRolesAsync(IEnumerable<string> userRoles)
    {
        // Solo Admin y Owner pueden gestionar roles
        return await HasSufficientAuthorityAsync(userRoles, RoleConstants.Names.Admin);
    }

    public async Task<bool> CanManageUserRoleAssignmentsAsync(IEnumerable<string> userRoles)
    {
        // Solo Moderator y superiores pueden gestionar asignaciones de roles
        return await HasSufficientAuthorityAsync(userRoles, RoleConstants.Names.Moderator);
    }
}