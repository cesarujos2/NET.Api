using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;

namespace NET.Api.Domain.Services;

/// <summary>
/// Implementación del servicio de dominio para la gestión de roles
/// </summary>
public class RoleManagementService : IRoleManagementService
{
    /// <summary>
    /// Verifica si un usuario puede ser asignado a un rol específico
    /// Regla de negocio: Solo se puede asignar roles de igual o menor jerarquía
    /// </summary>
    public bool CanAssignRole(IEnumerable<string> currentUserRoles, string targetRole)
    {
        if (!RoleConstants.IsValidRole(targetRole))
            return false;

        var highestUserRole = GetHighestRole(currentUserRoles);
        
        // El Owner puede asignar cualquier rol
        if (highestUserRole == RoleConstants.Names.Owner)
            return true;

        // Los demás solo pueden asignar roles de menor jerarquía
        return RoleConstants.GetRoleHierarchy(highestUserRole) > RoleConstants.GetRoleHierarchy(targetRole);
    }

    /// <summary>
    /// Verifica si un usuario puede remover un rol específico
    /// Regla de negocio: Solo se puede remover roles de menor jerarquía
    /// </summary>
    public bool CanRemoveRole(IEnumerable<string> currentUserRoles, string targetRole)
    {
        if (!RoleConstants.IsValidRole(targetRole))
            return false;

        var highestUserRole = GetHighestRole(currentUserRoles);
        
        // El Owner puede remover cualquier rol excepto Owner
        if (highestUserRole == RoleConstants.Names.Owner)
            return targetRole != RoleConstants.Names.Owner;

        // Los demás solo pueden remover roles de menor jerarquía
        return RoleConstants.GetRoleHierarchy(highestUserRole) > RoleConstants.GetRoleHierarchy(targetRole);
    }

    /// <summary>
    /// Obtiene los roles que un usuario puede asignar basado en sus roles actuales
    /// </summary>
    public IEnumerable<string> GetAssignableRoles(IEnumerable<string> currentUserRoles)
    {
        var highestUserRole = GetHighestRole(currentUserRoles);
        var userHierarchy = RoleConstants.GetRoleHierarchy(highestUserRole);

        return RoleConstants.AllRoles
            .Where(role => 
            {
                var roleHierarchy = RoleConstants.GetRoleHierarchy(role);
                
                // Owner puede asignar todos los roles
                if (highestUserRole == RoleConstants.Names.Owner)
                    return true;
                
                // Los demás solo pueden asignar roles de menor jerarquía
                return userHierarchy > roleHierarchy;
            });
    }

    /// <summary>
    /// Verifica si un conjunto de roles es válido para un usuario
    /// Regla de negocio: Un usuario no puede tener múltiples roles administrativos
    /// </summary>
    public bool IsValidRoleCombination(IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        
        // Verificar que todos los roles sean válidos
        if (roleList.Any(role => !RoleConstants.IsValidRole(role)))
            return false;

        // Un usuario no puede tener Owner y Admin al mismo tiempo
        if (roleList.Contains(RoleConstants.Names.Owner) && roleList.Contains(RoleConstants.Names.Admin))
            return false;

        // Un usuario no puede tener más de un rol administrativo
        var adminRolesCount = roleList.Count(role => RoleConstants.AdminRoles.Contains(role));
        if (adminRolesCount > 1)
            return false;

        return true;
    }

    /// <summary>
    /// Obtiene el rol con mayor jerarquía de una lista
    /// </summary>
    public string GetHighestRole(IEnumerable<string> roles)
    {
        return roles
            .Where(RoleConstants.IsValidRole)
            .OrderByDescending(RoleConstants.GetRoleHierarchy)
            .FirstOrDefault() ?? RoleConstants.Names.User;
    }

    /// <summary>
    /// Verifica si un usuario tiene autoridad suficiente para realizar una acción
    /// </summary>
    public bool HasSufficientAuthority(IEnumerable<string> userRoles, string requiredRole)
    {
        var highestUserRole = GetHighestRole(userRoles);
        return RoleConstants.HasSufficientAuthority(highestUserRole, requiredRole);
    }

    /// <summary>
    /// Valida que un rol puede ser creado
    /// </summary>
    public bool ValidateRoleCreation(ApplicationRole role)
    {
        // No se pueden crear roles del sistema
        if (RoleConstants.IsValidRole(role.Name!))
            return false;

        // El nombre no puede estar vacío
        if (string.IsNullOrWhiteSpace(role.Name))
            return false;

        // La descripción no puede estar vacía
        if (string.IsNullOrWhiteSpace(role.Description))
            return false;

        // El nivel de jerarquía debe ser válido (menor que User)
        if (role.HierarchyLevel >= RoleConstants.Hierarchy.User)
            return false;

        return true;
    }

    /// <summary>
    /// Valida que un rol puede ser eliminado
    /// </summary>
    public bool ValidateRoleDeletion(ApplicationRole role)
    {
        // No se pueden eliminar roles del sistema
        if (role.IsSystemRole)
            return false;

        // No se pueden eliminar roles activos con usuarios asignados
        // Esta validación se debe hacer en la capa de aplicación con acceso a datos
        
        return true;
    }
}