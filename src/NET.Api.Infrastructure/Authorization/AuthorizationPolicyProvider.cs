using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NET.Api.Infrastructure.Authorization.Requirements;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Authorization;

/// <summary>
/// Configurador de políticas de autorización
/// Centraliza la configuración de todas las políticas del sistema
/// </summary>
public static class AuthorizationPolicyProvider
{
    /// <summary>
    /// Configura todas las políticas de autorización
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    public static void ConfigureAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Políticas por rol específico
            ConfigureRoleSpecificPolicies(options);
            
            // Políticas por jerarquía
            ConfigureHierarchyPolicies(options);
            
            // Políticas por permisos
            ConfigurePermissionPolicies(options);
        });
    }

    /// <summary>
    /// Configura políticas para roles específicos
    /// </summary>
    private static void ConfigureRoleSpecificPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ApiConstants.Policies.RequireOwnerRole, policy =>
            policy.RequireRole(RoleConstants.Names.Owner));

        options.AddPolicy(ApiConstants.Policies.RequireAdminRole, policy =>
            policy.RequireRole(RoleConstants.Names.Admin));

        options.AddPolicy(ApiConstants.Policies.RequireModeratorRole, policy =>
            policy.RequireRole(RoleConstants.Names.Moderator));

        options.AddPolicy(ApiConstants.Policies.RequireSupportRole, policy =>
            policy.RequireRole(RoleConstants.Names.Support));

        options.AddPolicy(ApiConstants.Policies.RequireUserRole, policy =>
            policy.RequireRole(RoleConstants.Names.User));
    }

    /// <summary>
    /// Configura políticas basadas en jerarquía de roles
    /// </summary>
    private static void ConfigureHierarchyPolicies(AuthorizationOptions options)
    {
        // Roles elevados (Owner, Admin, Moderator)
        options.AddPolicy(ApiConstants.Policies.RequireElevatedRoles, policy =>
            policy.AddRequirements(new MultipleRolesRequirement(RoleConstants.ElevatedRoles)));

        // Admin o superior (Owner, Admin)
        options.AddPolicy(ApiConstants.Policies.RequireAdminOrAbove, policy =>
            policy.AddRequirements(new RoleHierarchyRequirement(RoleConstants.Names.Admin)));

        // Moderator o superior (Owner, Admin, Moderator)
        options.AddPolicy(ApiConstants.Policies.RequireModeratorOrAbove, policy =>
            policy.AddRequirements(new RoleHierarchyRequirement(RoleConstants.Names.Moderator)));
    }

    /// <summary>
    /// Configura políticas basadas en permisos específicos
    /// </summary>
    private static void ConfigurePermissionPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ApiConstants.Policies.CanManageUsers, policy =>
            policy.AddRequirements(new PermissionRequirement("CanManageUsers")));

        options.AddPolicy(ApiConstants.Policies.CanManageRoles, policy =>
            policy.AddRequirements(new PermissionRequirement("CanManageRoles")));

        options.AddPolicy(ApiConstants.Policies.CanViewReports, policy =>
            policy.AddRequirements(new PermissionRequirement("CanViewReports")));

        options.AddPolicy(ApiConstants.Policies.CanModerateContent, policy =>
            policy.AddRequirements(new PermissionRequirement("CanModerateContent")));

        options.AddPolicy(ApiConstants.Policies.CanAccessSupport, policy =>
            policy.AddRequirements(new PermissionRequirement("CanAccessSupport")));
    }

    /// <summary>
    /// Registra los handlers de autorización
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    public static void RegisterAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, Handlers.RoleHierarchyHandler>();
        services.AddScoped<IAuthorizationHandler, Handlers.MultipleRolesHandler>();
        services.AddScoped<IAuthorizationHandler, Handlers.PermissionHandler>();
    }
}