using Microsoft.AspNetCore.Identity;
using NET.Api.Shared.Constants;

namespace NET.Api.Domain.Entities;

/// <summary>
/// Entidad de dominio que extiende IdentityRole con funcionalidad adicional
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Descripción del rol
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de jerarquía del rol
    /// </summary>
    public int HierarchyLevel { get; set; }

    /// <summary>
    /// Indica si el rol está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indica si el rol es del sistema (no puede ser eliminado)
    /// </summary>
    public bool IsSystemRole { get; set; } = true;

    /// <summary>
    /// Fecha de creación del rol
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ApplicationRole() : base()
    {
    }

    /// <summary>
    /// Constructor con nombre de rol
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    public ApplicationRole(string roleName) : base(roleName)
    {
        SetRoleProperties(roleName);
    }

    /// <summary>
    /// Establece las propiedades del rol basado en su nombre
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    private void SetRoleProperties(string roleName)
    {
        HierarchyLevel = RoleConstants.GetRoleHierarchy(roleName);
        Description = GetRoleDescription(roleName);
        IsSystemRole = RoleConstants.IsValidRole(roleName);
    }

    /// <summary>
    /// Obtiene la descripción del rol
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>Descripción del rol</returns>
    private static string GetRoleDescription(string roleName)
    {
        return roleName?.ToUpperInvariant() switch
        {
            RoleConstants.Names.Owner => RoleConstants.Descriptions.Owner,
            RoleConstants.Names.Admin => RoleConstants.Descriptions.Admin,
            RoleConstants.Names.Moderator => RoleConstants.Descriptions.Moderator,
            RoleConstants.Names.Support => RoleConstants.Descriptions.Support,
            RoleConstants.Names.User => RoleConstants.Descriptions.User,
            _ => "Rol personalizado"
        };
    }

    /// <summary>
    /// Verifica si este rol tiene mayor o igual autoridad que otro
    /// </summary>
    /// <param name="otherRole">Otro rol a comparar</param>
    /// <returns>True si tiene suficiente autoridad</returns>
    public bool HasSufficientAuthority(ApplicationRole otherRole)
    {
        return HierarchyLevel >= otherRole.HierarchyLevel;
    }

    /// <summary>
    /// Verifica si este rol tiene mayor o igual autoridad que un nombre de rol
    /// </summary>
    /// <param name="roleName">Nombre del rol a comparar</param>
    /// <returns>True si tiene suficiente autoridad</returns>
    public bool HasSufficientAuthority(string roleName)
    {
        return RoleConstants.HasSufficientAuthority(Name!, roleName);
    }

    /// <summary>
    /// Actualiza la fecha de modificación
    /// </summary>
    public void UpdateModifiedDate()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activa el rol
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateModifiedDate();
    }

    /// <summary>
    /// Desactiva el rol (solo si no es del sistema)
    /// </summary>
    public void Deactivate()
    {
        if (!IsSystemRole)
        {
            IsActive = false;
            UpdateModifiedDate();
        }
    }
}