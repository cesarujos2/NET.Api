namespace NET.Api.Application.Common.Models.RoleManagement;

/// <summary>
/// DTO para representar un usuario con sus roles
/// </summary>
public class UserRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
    public string HighestRole { get; set; } = string.Empty;
    public int HighestRoleLevel { get; set; }
}