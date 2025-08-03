using NET.Api.Domain.Entities;

namespace NET.Api.Application.Abstractions.Services.IRoleService;

/// <summary>
/// Servicio de aplicación para consultas de roles
/// Responsabilidad única: Consultas y operaciones de lectura de roles
/// </summary>
public interface IRoleQueryService
{
    /// <summary>
    /// Obtiene todos los roles del sistema
    /// </summary>
    /// <returns>Lista de roles</returns>
    Task<IEnumerable<ApplicationRole>> GetAllRolesAsync();

    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <returns>Rol encontrado o null</returns>
    Task<ApplicationRole?> GetRoleByIdAsync(string roleId);

    /// <summary>
    /// Obtiene un rol por su nombre
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>Rol encontrado o null</returns>
    Task<ApplicationRole?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// Obtiene los roles que un usuario puede asignar
    /// </summary>
    /// <param name="currentUserRoles">Roles del usuario actual</param>
    /// <returns>Lista de roles asignables</returns>
    Task<IEnumerable<string>> GetAssignableRolesAsync(IEnumerable<string> currentUserRoles);

    /// <summary>
    /// Obtiene usuarios con múltiples roles asignados
    /// </summary>
    /// <returns>Lista de usuarios con múltiples roles</returns>
    Task<IEnumerable<ApplicationUser>> GetUsersWithMultipleRolesAsync();

    /// <summary>
    /// Obtiene los roles de un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Lista de roles del usuario</returns>
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);

    /// <summary>
    /// Obtiene roles activos (no eliminados)
    /// </summary>
    /// <returns>Lista de roles activos</returns>
    Task<IEnumerable<ApplicationRole>> GetActiveRolesAsync();

    /// <summary>
    /// Obtiene roles del sistema
    /// </summary>
    /// <returns>Lista de roles del sistema</returns>
    Task<IEnumerable<ApplicationRole>> GetSystemRolesAsync();

    /// <summary>
    /// Obtiene roles personalizados (no del sistema)
    /// </summary>
    /// <returns>Lista de roles personalizados</returns>
    Task<IEnumerable<ApplicationRole>> GetCustomRolesAsync();
}