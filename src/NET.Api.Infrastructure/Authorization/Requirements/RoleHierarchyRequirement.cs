using Microsoft.AspNetCore.Authorization;

namespace NET.Api.Infrastructure.Authorization.Requirements;

/// <summary>
/// Requirement para validar jerarquía de roles
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="requiredRole">Rol mínimo requerido</param>
public class RoleHierarchyRequirement(string requiredRole) : IAuthorizationRequirement
{
    /// <summary>
    /// Rol mínimo requerido
    /// </summary>
    public string RequiredRole { get; } = requiredRole;
}

/// <summary>
/// Requirement para validar múltiples roles permitidos
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="allowedRoles">Roles permitidos</param>
public class MultipleRolesRequirement(params string[] allowedRoles) : IAuthorizationRequirement
{
    /// <summary>
    /// Roles permitidos
    /// </summary>
    public string[] AllowedRoles { get; } = allowedRoles;
}

/// <summary>
/// Requirement para validar permisos específicos
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="permission">Permiso requerido</param>
public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    /// <summary>
    /// Permiso requerido
    /// </summary>
    public string Permission { get; } = permission;
}