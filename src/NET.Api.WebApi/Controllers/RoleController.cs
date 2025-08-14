using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;
using NET.Api.Application.Common.Exceptions;
using System.Security.Claims;
using NET.Api.Application.Abstractions.Services.IRoleService;

namespace NET.Api.WebApi.Controllers;

/// <summary>
/// Controlador refactorizado para gestión de roles siguiendo Clean Architecture
/// Utiliza los nuevos servicios separados por responsabilidades
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController(
    IRoleManagementService roleManagementService,
    IRoleQueryService roleQueryService) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los roles disponibles
    /// </summary>
    /// <returns>Lista de roles</returns>
    [HttpGet]
    [Authorize(Policy = ApiConstants.Policies.RequireModeratorOrAbove)]
    public async Task<ActionResult<IEnumerable<ApplicationRole>>> GetAllRoles()
    {
        var roles = await roleQueryService.GetAllRolesAsync();
        return Ok(new { success = true, data = roles, message = "Roles obtenidos exitosamente" });
    }

    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <returns>Información del rol</returns>
    [HttpGet("{roleId}")]
    [Authorize(Policy = ApiConstants.Policies.RequireElevatedRoles)]
    public async Task<ActionResult<ApplicationRole>> GetRoleById(string roleId)
    {
        var role = await roleQueryService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound(new { success = false, message = "Rol no encontrado" });
        }

        return Ok(new { success = true, data = role, message = "Rol obtenido exitosamente" });
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    /// <param name="request">Datos del rol a crear</param>
    /// <returns>Rol creado</returns>
    [HttpPost]
    [Authorize(Policy = ApiConstants.Policies.RequireAdminOrAbove)]
    public async Task<ActionResult<ApplicationRole>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var userRoles = GetUserRoles();
        
        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description,
            HierarchyLevel = request.HierarchyLevel,
            IsSystemRole = false
        };

        var createdRole = await roleManagementService.CreateRoleAsync(role, userRoles);
        return CreatedAtAction(nameof(GetRoleById), new { roleId = createdRole.Id }, 
            new { success = true, data = createdRole, message = "Rol creado exitosamente" });
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    /// <param name="roleId">ID del rol a actualizar</param>
    /// <param name="request">Nuevos datos del rol</param>
    /// <returns>Rol actualizado</returns>
    [HttpPut("{roleId}")]
    [Authorize(Policy = ApiConstants.Policies.RequireAdminOrAbove)]
    public async Task<ActionResult<ApplicationRole>> UpdateRole(string roleId, [FromBody] UpdateRoleRequest request)
    {
        var userRoles = GetUserRoles();
        
        var role = new ApplicationRole
        {
            Id = roleId,
            Description = request.Description,
            HierarchyLevel = request.HierarchyLevel
        };

        var updatedRole = await roleManagementService.UpdateRoleAsync(role, userRoles);
        return Ok(new { success = true, data = updatedRole, message = "Rol actualizado exitosamente" });
    }

    /// <summary>
    /// Elimina un rol
    /// </summary>
    /// <param name="roleId">ID del rol a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{roleId}")]
    [Authorize(Policy = ApiConstants.Policies.RequireAdminOrAbove)]
    public async Task<ActionResult> DeleteRole(string roleId)
    {
        var userRoles = GetUserRoles();
        await roleManagementService.DeleteRoleAsync(roleId, userRoles);
        return Ok(new { success = true, message = "Rol eliminado exitosamente" });
    }

    /// <summary>
    /// Asigna un rol a un usuario
    /// </summary>
    /// <param name="request">Datos de asignación de rol</param>
    /// <returns>Confirmación de asignación</returns>
    [HttpPost("assign")]
    [Authorize(Policy = ApiConstants.Policies.RequireModeratorOrAbove)]
    public async Task<ActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
    {
        var userRoles = GetUserRoles();
        await roleManagementService.AssignRoleToUserAsync(request.UserId, request.RoleName, userRoles);
        return Ok(new { success = true, message = "Rol asignado exitosamente" });
    }

    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    /// <param name="request">Datos de remoción de rol</param>
    /// <returns>Confirmación de remoción</returns>
    [HttpPost("remove")]
    [Authorize(Policy = ApiConstants.Policies.RequireModeratorOrAbove)]
    public async Task<ActionResult> RemoveRoleFromUser([FromBody] RemoveRoleRequest request)
    {
        var userRoles = GetUserRoles();
        await roleManagementService.RemoveRoleFromUserAsync(request.UserId, request.RoleName, userRoles);
        return Ok(new { success = true, message = "Rol removido exitosamente" });
    }

    /// <summary>
    /// Obtiene los roles que el usuario actual puede asignar
    /// </summary>
    /// <returns>Lista de roles asignables</returns>
    [HttpGet("assignable")]
    [Authorize(Policy = ApiConstants.Policies.RequireModeratorOrAbove)]
    public async Task<ActionResult<IEnumerable<string>>> GetAssignableRoles()
    {
        var userRoles = GetUserRoles();
        var assignableRoles = await roleQueryService.GetAssignableRolesAsync(userRoles);
        return Ok(new { success = true, data = assignableRoles, message = "Roles asignables obtenidos exitosamente" });
    }

    /// <summary>
    /// Obtiene los roles de un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Lista de roles del usuario</returns>
    [HttpGet("users/{userId}/roles")]
    [Authorize(Policy = ApiConstants.Policies.RequireModeratorOrAbove)]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
    {
        var roles = await roleQueryService.GetUserRolesAsync(userId);
        return Ok(new { success = true, data = roles, message = "Roles del usuario obtenidos exitosamente" });
    }

    /// <summary>
    /// Obtiene los roles del usuario actual desde el token JWT
    /// </summary>
    /// <returns>Lista de roles del usuario</returns>
    private IEnumerable<string> GetUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}

/// <summary>
/// Modelo para crear un nuevo rol
/// </summary>
public class CreateRoleRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int HierarchyLevel { get; set; }
}

/// <summary>
/// Modelo para actualizar un rol
/// </summary>
public class UpdateRoleRequest
{
    public required string Description { get; set; }
    public int HierarchyLevel { get; set; }
}

/// <summary>
/// Modelo para asignar un rol a un usuario
/// </summary>
public class AssignRoleRequest
{
    public required string UserId { get; set; }
    public required string RoleName { get; set; }
}

/// <summary>
/// Modelo para remover un rol de un usuario
/// </summary>
public class RemoveRoleRequest
{
    public required string UserId { get; set; }
    public required string RoleName { get; set; }
}