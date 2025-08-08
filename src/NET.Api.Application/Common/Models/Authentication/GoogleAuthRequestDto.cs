using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.Authentication;

public class GoogleAuthRequestDto
{
    /// <summary>
    /// Authorization code received from Google after user authentication
    /// </summary>
    [Required(ErrorMessage = "El código de autorización es requerido")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI that was used in the initial authorization request
    /// </summary>
    [Required(ErrorMessage = "La URI de redirección es requerida")]
    public string RedirectUri { get; set; } = string.Empty;
}
