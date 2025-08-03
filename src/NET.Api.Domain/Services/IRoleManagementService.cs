using NET.Api.Domain.Entities;

namespace NET.Api.Domain.Services;

/// <summary>
/// Servicio de dominio para la gestión de roles
/// Contiene la lógica de negocio pura relacionada con roles
/// </summary>
public interface IRoleManagementService : IDomainService
{
    /// <summary>
    /// Verifica si un usuario puede ser asignado a un rol específico
    /// </summary>
    /// <param name="currentUserRoles">Roles actuales del usuario que realiza la acción</param>
    /// <param name="targetRole">Rol que se quiere asignar</param>
    /// <returns>True si la asignación es válida</returns>
    bool CanAssignRole(IEnumerable<string> currentUserRoles, string targetRole);

    /// <summary>
    /// Verifica si un usuario puede remover un rol específico
    /// </summary>
    /// <param name="currentUserRoles">Roles actuales del usuario que realiza la acción</param>
    /// <param name="targetRole">Rol que se quiere remover</param>
    /// <returns>True si la remoción es válida</returns>
    bool CanRemoveRole(IEnumerable<string> currentUserRoles, string targetRole);

    /// <summary>
    /// Obtiene los roles que un usuario puede asignar basado en sus roles actuales
    /// </summary>
    /// <param name="currentUserRoles">Roles actuales del usuario</param>
    /// <returns>Lista de roles que puede asignar</returns>
    IEnumerable<string> GetAssignableRoles(IEnumerable<string> currentUserRoles);

    /// <summary>
    /// Verifica si un conjunto de roles es válido para un usuario
    /// </summary>
    /// <param name="roles">Roles a validar</param>
    /// <returns>True si la combinación de roles es válida</returns>
    bool IsValidRoleCombination(IEnumerable<string> roles);

    /// <summary>
    /// Obtiene el rol con mayor jerarquía de una lista
    /// </summary>
    /// <param name="roles">Lista de roles</param>
    /// <returns>Rol con mayor jerarquía</returns>
    string GetHighestRole(IEnumerable<string> roles);

    /// <summary>
    /// Verifica si un usuario tiene autoridad suficiente para realizar una acción
    /// </summary>
    /// <param name="userRoles">Roles del usuario</param>
    /// <param name="requiredRole">Rol mínimo requerido</param>
    /// <returns>True si tiene autoridad suficiente</returns>
    bool HasSufficientAuthority(IEnumerable<string> userRoles, string requiredRole);

    /// <summary>
    /// Valida que un rol puede ser creado
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol es válido para creación</returns>
    bool ValidateRoleCreation(ApplicationRole role);

    /// <summary>
    /// Valida que un rol puede ser eliminado
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol puede ser eliminado</returns>
    bool ValidateRoleDeletion(ApplicationRole role);
}