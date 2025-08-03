using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.RoleManagement;

/// <summary>
/// DTO para la solicitud de asignación de rol a un usuario
/// </summary>
public class AssignRoleRequestDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del rol es requerido")]
    public string RoleName { get; set; } = string.Empty;
}