using NET.Api.Domain.Entities;

namespace NET.Api.Domain.Services;

/// <summary>
/// Servicio de dominio para la gestión de jerarquías de roles
/// Responsabilidad única: Lógica pura de jerarquías de roles
/// </summary>
public interface IRoleHierarchyService : IDomainService
{
    /// <summary>
    /// Obtiene el nivel de jerarquía de un rol específico
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>Nivel de jerarquía del rol</returns>
    Task<int> GetRoleHierarchyLevelAsync(string roleName);

    /// <summary>
    /// Obtiene el rol con mayor jerarquía de una lista de roles
    /// </summary>
    /// <param name="roles">Lista de roles</param>
    /// <returns>Rol de mayor jerarquía</returns>
    Task<string> GetHighestRoleAsync(IEnumerable<string> roles);

    /// <summary>
    /// Verifica si un rol tiene mayor jerarquía que otro
    /// </summary>
    /// <param name="role1">Primer rol</param>
    /// <param name="role2">Segundo rol</param>
    /// <returns>True si role1 tiene mayor jerarquía que role2</returns>
    Task<bool> IsHigherThanAsync(string role1, string role2);

    /// <summary>
    /// Verifica si un rol tiene jerarquía igual o mayor que otro
    /// </summary>
    /// <param name="role1">Primer rol</param>
    /// <param name="role2">Segundo rol</param>
    /// <returns>True si role1 tiene jerarquía igual o mayor que role2</returns>
    Task<bool> IsEqualOrHigherThanAsync(string role1, string role2);

    /// <summary>
    /// Obtiene todos los roles que están por debajo de un rol específico en la jerarquía
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>Lista de roles subordinados</returns>
    Task<IEnumerable<string>> GetSubordinateRolesAsync(string roleName);

    /// <summary>
    /// Verifica si un rol es válido (existe en el sistema)
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>True si el rol es válido</returns>
    Task<bool> IsValidRoleAsync(string roleName);
}