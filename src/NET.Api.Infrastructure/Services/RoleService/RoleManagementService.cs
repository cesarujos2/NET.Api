using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Abstractions.Services.IRoleService;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services.RoleService;

/// <summary>
/// Implementación del servicio de aplicación para gestión de roles
/// Responsabilidad única: Orquestación de casos de uso de gestión de roles (CRUD)
/// </summary>
public class RoleManagementService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IRoleValidationService roleValidationService,
    IRoleAuthorizationService roleAuthorizationService,
    ILogger<RoleManagementService> logger) : IRoleManagementService
{
    public async Task<ApplicationRole> CreateRoleAsync(ApplicationRole role, IEnumerable<string> userRoles)
    {
        logger.LogInformation("Attempting to create role: {RoleName}", role.Name);

        // Validar autorización
        if (!await roleAuthorizationService.CanManageRolesAsync(userRoles))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to create role without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Insufficient authority to create roles");
        }

        // Validar jerarquía
        if (!await roleAuthorizationService.CanCreateRoleWithHierarchyAsync(userRoles, role.HierarchyLevel))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to create role with hierarchy level {HierarchyLevel} without sufficient authority", 
                string.Join(", ", userRoles), role.HierarchyLevel);
            throw new UnauthorizedAccessException("Cannot create role with this hierarchy level");
        }

        // Validar reglas de negocio
        if (!await roleValidationService.CanCreateRoleAsync(role))
        {
            logger.LogWarning("Role creation validation failed for role: {RoleName}", role.Name);
            
            // Determinar la razón específica del fallo
            if (string.IsNullOrWhiteSpace(role.Name))
                throw new BusinessRuleException("RoleNameRequired", "El nombre del rol es obligatorio.");
            
            if (string.IsNullOrWhiteSpace(role.Description))
                throw new BusinessRuleException("RoleDescriptionRequired", "La descripción del rol es obligatoria.");
            
            if (RoleConstants.IsStaticRole(role.Name))
                throw new BusinessRuleException("SystemRoleNameReserved", $"El nombre '{role.Name}' está reservado para roles del sistema y no puede ser utilizado.");
            
            if (!await roleValidationService.IsUniqueRoleNameAsync(role.Name))
                throw new BusinessRuleException("RoleNameNotUnique", $"Ya existe un rol con el nombre '{role.Name}'.");
            
            if (!await roleValidationService.IsValidHierarchyLevelAsync(role.HierarchyLevel))
                throw new BusinessRuleException("InvalidHierarchyLevel", "El nivel de jerarquía especificado no es válido.");
            
            throw new BusinessRuleException("RoleValidationFailed", "El rol no cumple con las reglas de validación requeridas.");
        }

        // Crear el rol
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create role {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Successfully created role: {RoleName}", role.Name);
        return role;
    }

    public async Task<ApplicationRole> UpdateRoleAsync(ApplicationRole role, IEnumerable<string> userRoles)
    {
        logger.LogInformation("Attempting to update role: {RoleId}", role.Id);

        // Validar autorización
        if (!await roleAuthorizationService.CanManageRolesAsync(userRoles))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to update role without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Insufficient authority to update roles");
        }

        // Obtener el rol actual
        var existingRole = await roleManager.FindByIdAsync(role.Id);
        if (existingRole == null)
        {
            logger.LogWarning("Attempted to update non-existent role: {RoleId}", role.Id);
            throw new NotFoundException($"Role with ID {role.Id} not found");
        }

        // Validar jerarquía
        if (!await roleAuthorizationService.CanUpdateRoleWithHierarchyAsync(userRoles, existingRole.HierarchyLevel, role.HierarchyLevel))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to update role hierarchy without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Cannot update role with this hierarchy level");
        }

        // Validar reglas de negocio
        if (!await roleValidationService.CanUpdateRoleAsync(role))
        {
            logger.LogWarning("Role update validation failed for role: {RoleId}", role.Id);
            
            // Determinar la razón específica del fallo
            if (existingRole.IsSystemRole)
                throw new BusinessRuleException("SystemRoleNotModifiable", $"El rol '{existingRole.Name}' es un rol del sistema y no puede ser modificado.");
            
            if (string.IsNullOrWhiteSpace(role.Description))
                throw new BusinessRuleException("RoleDescriptionRequired", "La descripción del rol es obligatoria.");
            
            if (!await roleValidationService.IsValidHierarchyLevelAsync(role.HierarchyLevel))
                throw new BusinessRuleException("InvalidHierarchyLevel", "El nivel de jerarquía especificado no es válido.");
            
            throw new BusinessRuleException("RoleValidationFailed", "El rol no cumple con las reglas de validación requeridas.");
        }

        // Actualizar propiedades
        existingRole.Description = role.Description;
        existingRole.HierarchyLevel = role.HierarchyLevel;

        var result = await roleManager.UpdateAsync(existingRole);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to update role {RoleId}: {Errors}", role.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Successfully updated role: {RoleId}", role.Id);
        return existingRole;
    }

    public async Task DeleteRoleAsync(string roleId, IEnumerable<string> userRoles)
    {
        logger.LogInformation("Attempting to delete role: {RoleId}", roleId);

        // Validar autorización
        if (!await roleAuthorizationService.CanManageRolesAsync(userRoles))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to delete role without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Insufficient authority to delete roles");
        }

        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            logger.LogWarning("Attempted to delete non-existent role: {RoleId}", roleId);
            throw new NotFoundException($"Role with ID {roleId} not found");
        }

        // Validar reglas de negocio
        if (!await roleValidationService.CanDeleteRoleAsync(role))
        {
            logger.LogWarning("Role deletion validation failed for role: {RoleId}", roleId);
            
            // Determinar la razón específica del fallo
            if (role.IsSystemRole)
                throw new BusinessRuleException("SystemRoleNotDeletable", $"El rol '{role.Name}' es un rol del sistema y no puede ser eliminado.");
            
            if (!await roleValidationService.HasNoUsersAssignedAsync(role.Name!))
                throw new BusinessRuleException("RoleHasAssignedUsers", $"No se puede eliminar el rol '{role.Name}' porque tiene usuarios asignados. Primero debe remover el rol de todos los usuarios.");
            
            throw new BusinessRuleException("RoleDeletionNotAllowed", "El rol no puede ser eliminado debido a restricciones del sistema.");
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to delete role {RoleId}: {Errors}", roleId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Successfully deleted role: {RoleId}", roleId);
    }

    public async Task AssignRoleToUserAsync(string userId, string roleName, IEnumerable<string> userRoles)
    {
        logger.LogInformation("Attempting to assign role {RoleName} to user {UserId}", roleName, userId);

        // Validar autorización para gestionar asignaciones
        if (!await roleAuthorizationService.CanManageUserRoleAssignmentsAsync(userRoles))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to assign role without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Insufficient authority to assign roles");
        }

        // Validar autorización para asignar este rol específico
        if (!await roleAuthorizationService.CanAssignRoleAsync(userRoles, roleName))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to assign role {RoleName} without sufficient authority", 
                string.Join(", ", userRoles), roleName);
            throw new UnauthorizedAccessException($"Cannot assign role {roleName}");
        }

        // Validar límite de Owners si se está asignando el rol Owner
        if (roleName == RoleConstants.Names.Owner)
        {
            if (!await roleValidationService.CanAssignOwnerRoleAsync())
            {
                logger.LogWarning("Attempted to assign Owner role but maximum limit of {MaxOwners} would be exceeded", 
                    RoleConstants.Limits.MaxOwners);
                throw new BusinessRuleException("MaxOwnersExceeded", 
                    $"No se puede asignar el rol Owner. El límite máximo es de {RoleConstants.Limits.MaxOwners} propietarios.");
            }
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Attempted to assign role to non-existent user: {UserId}", userId);
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to assign role {RoleName} to user {UserId}: {Errors}", 
                roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Successfully assigned role {RoleName} to user {UserId}", roleName, userId);
    }

    public async Task RemoveRoleFromUserAsync(string userId, string roleName, IEnumerable<string> userRoles)
    {
        logger.LogInformation("Attempting to remove role {RoleName} from user {UserId}", roleName, userId);

        // Validar autorización para gestionar asignaciones
        if (!await roleAuthorizationService.CanManageUserRoleAssignmentsAsync(userRoles))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to remove role without sufficient authority", string.Join(", ", userRoles));
            throw new UnauthorizedAccessException("Insufficient authority to remove roles");
        }

        // Validar autorización para remover este rol específico
        if (!await roleAuthorizationService.CanRemoveRoleAsync(userRoles, roleName))
        {
            logger.LogWarning("User with roles {UserRoles} attempted to remove role {RoleName} without sufficient authority", 
                string.Join(", ", userRoles), roleName);
            throw new UnauthorizedAccessException($"Cannot remove role {roleName}");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Attempted to remove role from non-existent user: {UserId}", userId);
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to remove role {RoleName} from user {UserId}: {Errors}", 
                roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Successfully removed role {RoleName} from user {UserId}", roleName, userId);
    }
}