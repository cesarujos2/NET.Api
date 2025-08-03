using Microsoft.AspNetCore.Authorization;
using NET.Api.Domain.Services;
using NET.Api.Infrastructure.Authorization.Requirements;
using System.Security.Claims;

namespace NET.Api.Infrastructure.Authorization.Handlers;

/// <summary>
/// Handler para validar jerarquía de roles
/// </summary>
public class RoleHierarchyHandler(IRoleManagementService roleManagementService) : AuthorizationHandler<RoleHierarchyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleHierarchyRequirement requirement)
    {

        var userRoles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        if (roleManagementService.HasSufficientAuthority(userRoles, requirement.RequiredRole))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar múltiples roles
/// </summary>
public class MultipleRolesHandler : AuthorizationHandler<MultipleRolesRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultipleRolesRequirement requirement)
    {
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (userRoles.Any(role => requirement.AllowedRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar permisos específicos
/// </summary>
public class PermissionHandler(IRoleManagementService roleManagementService) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Si el usuario no está autenticado, fallar inmediatamente
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        // Lógica de permisos basada en roles
        var hasPermission = requirement.Permission switch
        {
            "CanManageUsers" => roleManagementService.HasSufficientAuthority(userRoles, "Admin"),
            "CanManageRoles" => roleManagementService.HasSufficientAuthority(userRoles, "Owner"),
            "CanViewReports" => roleManagementService.HasSufficientAuthority(userRoles, "Moderator"),
            "CanModerateContent" => roleManagementService.HasSufficientAuthority(userRoles, "Moderator"),
            "CanAccessSupport" => roleManagementService.HasSufficientAuthority(userRoles, "Support"),
            _ => false
        };

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}