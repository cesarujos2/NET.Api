using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.UserAccount;

/// <summary>
/// DTO for creating a new user account
/// </summary>
public class CreateUserAccountRequestDto
{
    /// <summary>
    /// Display name for this account
    /// </summary>
    [Required(ErrorMessage = "El nombre de la cuenta es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre de la cuenta no puede exceder 100 caracteres.")]
    public string AccountName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description or purpose of this account
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Profile picture URL for this account
    /// </summary>
    [StringLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres.")]
    [Url(ErrorMessage = "La URL de la imagen no es válida.")]
    public string? ProfilePictureUrl { get; set; }
    
    /// <summary>
    /// Indicates if this should be the default account
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Display order for account selection
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El orden de visualización debe ser un número positivo.")]
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Initial settings for the account (JSON format)
    /// </summary>
    public string? Settings { get; set; }
}