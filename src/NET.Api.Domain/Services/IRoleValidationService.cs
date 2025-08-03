using NET.Api.Domain.Entities;

namespace NET.Api.Domain.Services;

/// <summary>
/// Servicio de dominio para validaciones de roles
/// Responsabilidad única: Validaciones de reglas de negocio de roles
/// </summary>
public interface IRoleValidationService : IDomainService
{
    /// <summary>
    /// Valida que un rol puede ser creado según las reglas de negocio
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol puede ser creado</returns>
    Task<bool> CanCreateRoleAsync(ApplicationRole role);

    /// <summary>
    /// Valida que un rol puede ser eliminado según las reglas de negocio
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol puede ser eliminado</returns>
    Task<bool> CanDeleteRoleAsync(ApplicationRole role);

    /// <summary>
    /// Valida que un rol puede ser actualizado según las reglas de negocio
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol puede ser actualizado</returns>
    Task<bool> CanUpdateRoleAsync(ApplicationRole role);

    /// <summary>
    /// Valida que el nombre de un rol es válido según las reglas de negocio
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>True si el nombre es válido</returns>
    Task<bool> IsValidRoleNameAsync(string roleName);

    /// <summary>
    /// Valida que el nivel de jerarquía es válido para un rol
    /// </summary>
    /// <param name="hierarchyLevel">Nivel de jerarquía</param>
    /// <returns>True si el nivel es válido</returns>
    Task<bool> IsValidHierarchyLevelAsync(int hierarchyLevel);

    /// <summary>
    /// Valida que un rol no está duplicado en el sistema
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <param name="excludeRoleId">ID del rol a excluir de la validación (para actualizaciones)</param>
    /// <returns>True si el rol no está duplicado</returns>
    Task<bool> IsUniqueRoleNameAsync(string roleName, string? excludeRoleId = null);

    /// <summary>
    /// Valida que un rol del sistema no puede ser modificado
    /// </summary>
    /// <param name="role">Rol a validar</param>
    /// <returns>True si el rol puede ser modificado (no es del sistema)</returns>
    Task<bool> CanModifySystemRoleAsync(ApplicationRole role);

    /// <summary>
    /// Valida que un rol no tiene usuarios asignados
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>True si el rol no tiene usuarios asignados</returns>
    Task<bool> HasNoUsersAssignedAsync(string roleName);

    /// <summary>
    /// Verifica si se puede asignar el rol Owner sin exceder el límite máximo
    /// </summary>
    /// <param name="excludeUserId">ID del usuario a excluir del conteo (para actualizaciones)</param>
    /// <returns>True si no se excede el límite de Owners</returns>
    Task<bool> CanAssignOwnerRoleAsync(string? excludeUserId = null);
}