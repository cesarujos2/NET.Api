using Microsoft.AspNetCore.Identity;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services.RoleService;

/// <summary>
/// Implementación del servicio de dominio para validaciones de roles
/// Responsabilidad única: Validaciones de reglas de negocio de roles
/// </summary>
public class RoleValidationService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager) : IRoleValidationService
{
    public async Task<bool> CanCreateRoleAsync(ApplicationRole role)
    {
        // Verificar que el nombre no esté vacío
        if (string.IsNullOrWhiteSpace(role.Name))
            return false;

        // Verificar que la descripción no esté vacía
        if (string.IsNullOrWhiteSpace(role.Description))
            return false;

        // No se pueden crear roles con nombres de roles del sistema
        if (RoleConstants.IsStaticRole(role.Name))
            return false;

        // Verificar que el nombre sea único
        if (!await IsUniqueRoleNameAsync(role.Name))
            return false;

        // Verificar que el nivel de jerarquía sea válido
        if (!await IsValidHierarchyLevelAsync(role.HierarchyLevel))
            return false;

        return true;
    }

    public async Task<bool> CanDeleteRoleAsync(ApplicationRole role)
    {
        // No se pueden eliminar roles del sistema
        if (role.IsSystemRole)
            return false;

        // No se pueden eliminar roles que tienen usuarios asignados
        if (!await HasNoUsersAssignedAsync(role.Name!))
            return false;

        return true;
    }

    public async Task<bool> CanUpdateRoleAsync(ApplicationRole role)
    {
        // No se pueden modificar roles del sistema
        if (!await CanModifySystemRoleAsync(role))
            return false;

        // Verificar que la descripción no esté vacía
        if (string.IsNullOrWhiteSpace(role.Description))
            return false;

        // Verificar que el nivel de jerarquía sea válido
        if (!await IsValidHierarchyLevelAsync(role.HierarchyLevel))
            return false;

        return true;
    }

    public Task<bool> IsValidRoleNameAsync(string roleName)
    {
        // El nombre no puede estar vacío
        if (string.IsNullOrWhiteSpace(roleName))
            return Task.FromResult(false);

        // El nombre no puede contener caracteres especiales
        if (roleName.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-'))
            return Task.FromResult(false);

        // El nombre debe tener una longitud mínima y máxima
        if (roleName.Length < RoleConstants.Limits.MinRoleNameLength || roleName.Length > RoleConstants.Limits.MaxRoleNameLength)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public Task<bool> IsValidHierarchyLevelAsync(int hierarchyLevel)
    {
        // El nivel de jerarquía debe estar en un rango válido
        // Los roles personalizados deben tener jerarquía menor que User (1)
        return Task.FromResult(hierarchyLevel >= 0 && hierarchyLevel < RoleConstants.Hierarchy.User);
    }

    public async Task<bool> IsUniqueRoleNameAsync(string roleName, string? excludeRoleId = null)
    {
        var existingRole = await roleManager.FindByNameAsync(roleName);
        
        if (existingRole == null)
            return true;

        // Si estamos excluyendo un rol específico (para actualizaciones)
        if (!string.IsNullOrEmpty(excludeRoleId) && existingRole.Id == excludeRoleId)
            return true;

        return false;
    }

    public Task<bool> CanModifySystemRoleAsync(ApplicationRole role)
    {
        // Los roles del sistema no pueden ser modificados
        return Task.FromResult(!role.IsSystemRole);
    }

    public async Task<bool> HasNoUsersAssignedAsync(string roleName)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
        return !usersInRole.Any();
    }

    /// <summary>
    /// Verifica si se puede asignar el rol Owner sin exceder el límite máximo
    /// </summary>
    /// <param name="excludeUserId">ID del usuario a excluir del conteo (para actualizaciones)</param>
    /// <returns>True si no se excede el límite de Owners</returns>
    public async Task<bool> CanAssignOwnerRoleAsync(string? excludeUserId = null)
    {
        var ownersInRole = await userManager.GetUsersInRoleAsync(RoleConstants.Names.Owner);
        
        // Si estamos excluyendo un usuario específico (para actualizaciones)
        if (!string.IsNullOrEmpty(excludeUserId))
        {
            ownersInRole = ownersInRole.Where(u => u.Id != excludeUserId).ToList();
        }
        
        return ownersInRole.Count < RoleConstants.Limits.MaxOwners;
    }
}