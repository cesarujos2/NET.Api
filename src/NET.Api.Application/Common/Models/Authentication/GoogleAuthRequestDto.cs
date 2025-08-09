using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.Authentication;

public class GoogleAuthRequestDto
{
    /// <summary>
    /// Authorization code received from Google after user authentication
    /// </summary>
    [Required(ErrorMessage = "El c�digo de autorizaci�n es requerido")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI that was used in the initial authorization request
    /// </summary>
    [Required(ErrorMessage = "La URI de redirecci�n es requerida")]
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// CSRF state parameter for security validation
    /// </summary>
    [Required(ErrorMessage = "El parámetro de estado CSRF es requerido")]
    public string State { get; set; } = string.Empty;
}
