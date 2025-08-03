using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder avanzado para roles del sistema
/// Utiliza el sistema de roles mejorado con ApplicationRole
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Siembra los roles del sistema de manera avanzada
    /// </summary>
    /// <param name="roleManager">Gestor de roles de Identity</param>
    /// <param name="logger">Logger para registro de eventos</param>
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        logger.LogInformation("Iniciando seeding de roles del sistema...");

        var rolesToSeed = GetSystemRoles();

        foreach (var roleData in rolesToSeed)
        {
            await SeedRoleAsync(roleManager, roleData, logger);
        }

        logger.LogInformation("Seeding de roles completado exitosamente.");
    }

    /// <summary>
    /// Obtiene la configuración de roles del sistema
    /// </summary>
    /// <returns>Lista de roles con su configuración</returns>
    private static List<RoleConfiguration> GetSystemRoles()
    {
        return
        [
            new()
            {
                Name = RoleConstants.Names.Owner,
                Description = RoleConstants.Descriptions.Owner,
                HierarchyLevel = RoleConstants.Hierarchy.Owner,
                IsSystemRole = true,
                IsActive = true
            },
            new()
            {
                Name = RoleConstants.Names.Admin,
                Description = RoleConstants.Descriptions.Admin,
                HierarchyLevel = RoleConstants.Hierarchy.Admin,
                IsSystemRole = true,
                IsActive = true
            },
            new()
            {
                Name = RoleConstants.Names.Moderator,
                Description = RoleConstants.Descriptions.Moderator,
                HierarchyLevel = RoleConstants.Hierarchy.Moderator,
                IsSystemRole = true,
                IsActive = true
            },
            new()
            {
                Name = RoleConstants.Names.Support,
                Description = RoleConstants.Descriptions.Support,
                HierarchyLevel = RoleConstants.Hierarchy.Support,
                IsSystemRole = true,
                IsActive = true
            },
            new()
            {
                Name = RoleConstants.Names.User,
                Description = RoleConstants.Descriptions.User,
                HierarchyLevel = RoleConstants.Hierarchy.User,
                IsSystemRole = true,
                IsActive = true
            }
        ];
    }

    /// <summary>
    /// Siembra un rol específico
    /// </summary>
    /// <param name="roleManager">Gestor de roles</param>
    /// <param name="roleConfig">Configuración del rol</param>
    /// <param name="logger">Logger</param>
    private static async Task SeedRoleAsync(
        RoleManager<ApplicationRole> roleManager, 
        RoleConfiguration roleConfig, 
        ILogger logger)
    {
        var existingRole = await roleManager.FindByNameAsync(roleConfig.Name);
        
        if (existingRole == null)
        {
            // Crear nuevo rol
            var newRole = new ApplicationRole(roleConfig.Name)
            {
                Description = roleConfig.Description,
                HierarchyLevel = roleConfig.HierarchyLevel,
                IsSystemRole = roleConfig.IsSystemRole,
                IsActive = roleConfig.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await roleManager.CreateAsync(newRole);
            
            if (result.Succeeded)
            {
                logger.LogInformation(
                    "Rol '{RoleName}' creado exitosamente. Jerarquía: {Hierarchy}, Descripción: '{Description}'",
                    roleConfig.Name, roleConfig.HierarchyLevel, roleConfig.Description);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError(
                    "Error al crear el rol '{RoleName}': {Errors}",
                    roleConfig.Name, errors);
            }
        }
        else
        {
            // Actualizar rol existente si es necesario
            var needsUpdate = false;

            if (existingRole.Description != roleConfig.Description)
            {
                existingRole.Description = roleConfig.Description;
                needsUpdate = true;
            }

            if (existingRole.HierarchyLevel != roleConfig.HierarchyLevel)
            {
                existingRole.HierarchyLevel = roleConfig.HierarchyLevel;
                needsUpdate = true;
            }

            if (existingRole.IsSystemRole != roleConfig.IsSystemRole)
            {
                existingRole.IsSystemRole = roleConfig.IsSystemRole;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                existingRole.UpdateModifiedDate();
                var result = await roleManager.UpdateAsync(existingRole);
                
                if (result.Succeeded)
                {
                    logger.LogInformation(
                        "Rol '{RoleName}' actualizado exitosamente.",
                        roleConfig.Name);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError(
                        "Error al actualizar el rol '{RoleName}': {Errors}",
                        roleConfig.Name, errors);
                }
            }
            else
            {
                logger.LogInformation(
                    "Rol '{RoleName}' ya existe y está actualizado.",
                    roleConfig.Name);
            }
        }
    }

    /// <summary>
    /// Configuración para un rol del sistema
    /// </summary>
    private class RoleConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
    }
}