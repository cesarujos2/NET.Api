using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Shared.Constants;
using NET.Api.Shared.Models;

namespace NET.Api.WebApi.Controllers;

/// <summary>
/// Controlador de ejemplo que demuestra el uso del sistema avanzado de roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación básica
public class RoleManagementController : ControllerBase
{
    /// <summary>
    /// Endpoint que solo pueden acceder usuarios con rol Owner
    /// </summary>
    [HttpGet("owner-only")]
    [Authorize(Policy = ApiConstants.Policies.RequireOwnerRole)]
    public IActionResult OwnerOnlyEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Solo Owner",
            Data = "Este endpoint solo es accesible por usuarios con rol Owner"
        });
    }

    /// <summary>
    /// Endpoint que pueden acceder usuarios con rol Admin o superior (Owner, Admin)
    /// </summary>
    [HttpGet("admin-or-above")]
    [Authorize(Policy = ApiConstants.Policies.RequireAdminOrAbove)]
    public IActionResult AdminOrAboveEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Admin o superior",
            Data = "Este endpoint es accesible por usuarios con rol Admin o superior"
        });
    }

    /// <summary>
    /// Endpoint que pueden acceder usuarios con roles elevados (Owner, Admin, Moderator)
    /// </summary>
    [HttpGet("elevated-roles")]
    [Authorize(Policy = ApiConstants.Policies.RequireElevatedRoles)]
    public IActionResult ElevatedRolesEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Roles elevados",
            Data = "Este endpoint es accesible por usuarios con roles elevados (Owner, Admin, Moderator)"
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso específico para gestionar usuarios
    /// </summary>
    [HttpPost("manage-users")]
    [Authorize(Policy = ApiConstants.Policies.CanManageUsers)]
    public IActionResult ManageUsersEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Gestión de usuarios",
            Data = "Este endpoint permite gestionar usuarios (requiere permiso CanManageUsers)"
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso específico para gestionar roles
    /// </summary>
    [HttpPost("manage-roles")]
    [Authorize(Policy = ApiConstants.Policies.CanManageRoles)]
    public IActionResult ManageRolesEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Gestión de roles",
            Data = "Este endpoint permite gestionar roles (requiere permiso CanManageRoles)"
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso específico para ver reportes
    /// </summary>
    [HttpGet("reports")]
    [Authorize(Policy = ApiConstants.Policies.CanViewReports)]
    public IActionResult ViewReportsEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Ver reportes",
            Data = "Este endpoint permite ver reportes (requiere permiso CanViewReports)"
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso específico para moderar contenido
    /// </summary>
    [HttpPost("moderate-content")]
    [Authorize(Policy = ApiConstants.Policies.CanModerateContent)]
    public IActionResult ModerateContentEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Moderación de contenido",
            Data = "Este endpoint permite moderar contenido (requiere permiso CanModerateContent)"
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso específico para acceder a soporte
    /// </summary>
    [HttpGet("support")]
    [Authorize(Policy = ApiConstants.Policies.CanAccessSupport)]
    public IActionResult AccessSupportEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Acceso a soporte",
            Data = "Este endpoint permite acceder a funciones de soporte (requiere permiso CanAccessSupport)"
        });
    }

    /// <summary>
    /// Endpoint que demuestra el uso de múltiples roles específicos
    /// </summary>
    [HttpGet("support-or-moderator")]
    [Authorize(Roles = $"{RoleConstants.Names.Support},{RoleConstants.Names.Moderator}")]
    public IActionResult SupportOrModeratorEndpoint()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Acceso exitoso - Support o Moderator",
            Data = "Este endpoint es accesible por usuarios con rol Support o Moderator"
        });
    }

    /// <summary>
    /// Endpoint público para obtener información sobre los roles del sistema
    /// </summary>
    [HttpGet("roles-info")]
    [AllowAnonymous]
    public IActionResult GetRolesInfo()
    {
        var rolesInfo = new
        {
            AvailableRoles = new[]
            {
                new { Name = RoleConstants.Names.Owner, Description = RoleConstants.Descriptions.Owner, Level = RoleConstants.Hierarchy.Owner },
                new { Name = RoleConstants.Names.Admin, Description = RoleConstants.Descriptions.Admin, Level = RoleConstants.Hierarchy.Admin },
                new { Name = RoleConstants.Names.Moderator, Description = RoleConstants.Descriptions.Moderator, Level = RoleConstants.Hierarchy.Moderator },
                new { Name = RoleConstants.Names.Support, Description = RoleConstants.Descriptions.Support, Level = RoleConstants.Hierarchy.Support },
                new { Name = RoleConstants.Names.User, Description = RoleConstants.Descriptions.User, Level = RoleConstants.Hierarchy.User }
            },
            ElevatedRoles = RoleConstants.ElevatedRoles,
            AdminRoles = RoleConstants.AdminRoles
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Información de roles obtenida exitosamente",
            Data = rolesInfo
        });
    }
}