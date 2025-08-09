using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.Authentication;

/// <summary>
/// Request DTO for selecting a specific account after initial login
/// </summary>
public class SelectAccountRequestDto
{
    /// <summary>
    /// The ID of the selected user account
    /// </summary>
    [Required(ErrorMessage = "El ID de la cuenta es requerido.")]
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// The temporary selection token received during initial login
    /// </summary>
    [Required(ErrorMessage = "El token de selección es requerido.")]
    public string SelectionToken { get; set; } = string.Empty;
}