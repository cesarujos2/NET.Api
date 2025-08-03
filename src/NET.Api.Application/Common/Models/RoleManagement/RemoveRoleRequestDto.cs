using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.RoleManagement;

/// <summary>
/// DTO para la solicitud de remoción de rol de un usuario
/// </summary>
public class RemoveRoleRequestDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del rol es requerido")]
    public string RoleName { get; set; } = string.Empty;
}