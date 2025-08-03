using NET.Api.Domain.Entities;

namespace NET.Api.Application.Abstractions.Services.IRoleService;

/// <summary>
/// Servicio de aplicación para la gestión de roles
/// Responsabilidad única: Orquestación de casos de uso de gestión de roles (CRUD)
/// </summary>
public interface IRoleManagementService
{
    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    /// <param name="role">Rol a crear</param>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <returns>Rol creado</returns>
    Task<ApplicationRole> CreateRoleAsync(ApplicationRole role, IEnumerable<string> userRoles);

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    /// <param name="role">Rol con los nuevos datos</param>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <returns>Rol actualizado</returns>
    Task<ApplicationRole> UpdateRoleAsync(ApplicationRole role, IEnumerable<string> userRoles);

    /// <summary>
    /// Elimina un rol
    /// </summary>
    /// <param name="roleId">ID del rol a eliminar</param>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <returns>Task completado</returns>
    Task DeleteRoleAsync(string roleId, IEnumerable<string> userRoles);

    /// <summary>
    /// Asigna un rol a un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="roleName">Nombre del rol</param>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <returns>Task completado</returns>
    Task AssignRoleToUserAsync(string userId, string roleName, IEnumerable<string> userRoles);

    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="roleName">Nombre del rol</param>
    /// <param name="userRoles">Roles del usuario que realiza la acción</param>
    /// <returns>Task completado</returns>
    Task RemoveRoleFromUserAsync(string userId, string roleName, IEnumerable<string> userRoles);
}