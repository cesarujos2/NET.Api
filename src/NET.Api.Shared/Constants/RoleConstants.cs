namespace NET.Api.Shared.Constants;

/// <summary>
/// Constantes de roles del sistema
/// Centraliza la definición de roles para evitar strings mágicos
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Roles del sistema
    /// </summary>
    public static class Names
    {
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
        public const string Support = "Support";
    }

    /// <summary>
    /// Descripciones de roles para UI y documentación
    /// </summary>
    public static class Descriptions
    {
        public const string Owner = "Propietario del sistema con acceso completo";
        public const string Admin = "Administrador con permisos de gestión";
        public const string User = "Usuario estándar del sistema";
        public const string Moderator = "Moderador de contenido";
        public const string Support = "Soporte técnico";
    }

    /// <summary>
    /// Jerarquía de roles (nivel de autoridad)
    /// Valores más altos = mayor autoridad
    /// </summary>
    public static class Hierarchy
    {
        public const int Owner = 100;
        public const int Admin = 80;
        public const int Moderator = 60;
        public const int Support = 40;
        public const int User = 20;
    }

    /// <summary>
    /// Obtiene todos los roles disponibles
    /// </summary>
    public static readonly string[] AllRoles = 
    {
        Names.Owner,
        Names.Admin,
        Names.Moderator,
        Names.Support,
        Names.User
    };

    /// <summary>
    /// Roles administrativos
    /// </summary>
    public static readonly string[] AdminRoles = 
    {
        Names.Owner,
        Names.Admin
    };

    /// <summary>
    /// Roles con permisos elevados
    /// </summary>
    public static readonly string[] ElevatedRoles = 
    {
        Names.Owner,
        Names.Admin,
        Names.Moderator
    };

    /// <summary>
    /// Verifica si un rol es válido
    /// </summary>
    /// <param name="role">Nombre del rol</param>
    /// <returns>True si el rol es válido</returns>
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene el nivel de jerarquía de un rol
    /// </summary>
    /// <param name="role">Nombre del rol</param>
    /// <returns>Nivel de jerarquía o 0 si no es válido</returns>
    public static int GetRoleHierarchy(string role)
    {
        return role switch
        {
            Names.Owner => Hierarchy.Owner,
            Names.Admin => Hierarchy.Admin,
            Names.Moderator => Hierarchy.Moderator,
            Names.Support => Hierarchy.Support,
            Names.User => Hierarchy.User,
            _ => 0
        };
    }

    /// <summary>
    /// Verifica si un rol tiene mayor o igual autoridad que otro
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <param name="requiredRole">Rol requerido</param>
    /// <returns>True si tiene suficiente autoridad</returns>
    public static bool HasSufficientAuthority(string role, string requiredRole)
    {
        return GetRoleHierarchy(role) >= GetRoleHierarchy(requiredRole);
    }
}