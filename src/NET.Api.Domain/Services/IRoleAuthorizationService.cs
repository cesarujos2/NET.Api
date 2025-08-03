namespace NET.Api.Domain.Services;

/// <summary>
/// Servicio de dominio para autorización de roles
/// Responsabilidad única: Lógica de autorización y permisos de roles
/// </summary>
public interface IRoleAuthorizationService : IDomainService
{
    /// <summary>
    /// Verifica si un usuario puede asignar un rol específico
    /// </summary>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <param name="targetRole">Rol que se quiere asignar</param>
    /// <returns>True si puede asignar el rol</returns>
    Task<bool> CanAssignRoleAsync(IEnumerable<string> userRoles, string targetRole);

    /// <summary>
    /// Verifica si un usuario puede remover un rol específico
    /// </summary>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <param name="targetRole">Rol que se quiere remover</param>
    /// <returns>True si puede remover el rol</returns>
    Task<bool> CanRemoveRoleAsync(IEnumerable<string> userRoles, string targetRole);

    /// <summary>
    /// Verifica si un usuario tiene autoridad suficiente para realizar una acción
    /// </summary>
    /// <param name="userRoles">Roles del usuario</param>
    /// <param name="requiredRole">Rol mínimo requerido para la acción</param>
    /// <returns>True si tiene autoridad suficiente</returns>
    Task<bool> HasSufficientAuthorityAsync(IEnumerable<string> userRoles, string requiredRole);

    /// <summary>
    /// Verifica si un usuario puede crear un rol con un nivel de jerarquía específico
    /// </summary>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <param name="targetHierarchyLevel">Nivel de jerarquía del rol a crear</param>
    /// <returns>True si puede crear el rol</returns>
    Task<bool> CanCreateRoleWithHierarchyAsync(IEnumerable<string> userRoles, int targetHierarchyLevel);

    /// <summary>
    /// Verifica si un usuario puede actualizar un rol con un nivel de jerarquía específico
    /// </summary>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <param name="currentRoleHierarchy">Nivel de jerarquía actual del rol</param>
    /// <param name="newHierarchyLevel">Nuevo nivel de jerarquía del rol</param>
    /// <returns>True si puede actualizar el rol</returns>
    Task<bool> CanUpdateRoleWithHierarchyAsync(IEnumerable<string> userRoles, int currentRoleHierarchy, int newHierarchyLevel);

    /// <summary>
    /// Obtiene los roles que un usuario puede asignar basado en sus roles actuales
    /// </summary>
    /// <param name="userRoles">Roles del usuario</param>
    /// <returns>Lista de roles que puede asignar</returns>
    Task<IEnumerable<string>> GetAssignableRolesAsync(IEnumerable<string> userRoles);

    /// <summary>
    /// Verifica si un usuario puede gestionar roles (crear, actualizar, eliminar)
    /// </summary>
    /// <param name="userRoles">Roles del usuario</param>
    /// <returns>True si puede gestionar roles</returns>
    Task<bool> CanManageRolesAsync(IEnumerable<string> userRoles);

    /// <summary>
    /// Verifica si un usuario puede asignar/remover roles de otros usuarios
    /// </summary>
    /// <param name="userRoles">Roles del usuario</param>
    /// <returns>True si puede gestionar asignaciones de roles</returns>
    Task<bool> CanManageUserRoleAssignmentsAsync(IEnumerable<string> userRoles);
}