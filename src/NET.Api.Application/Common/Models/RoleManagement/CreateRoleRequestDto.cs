using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.RoleManagement;

/// <summary>
/// DTO para la solicitud de creación de un nuevo rol
/// </summary>
public class CreateRoleRequestDto
{
    [Required(ErrorMessage = "El nombre del rol es requerido")]
    [StringLength(256, ErrorMessage = "El nombre del rol no puede exceder 256 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del rol es requerida")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "El nivel de jerarquía debe estar entre 1 y 100")]
    public int HierarchyLevel { get; set; }

    public bool IsActive { get; set; } = true;
}