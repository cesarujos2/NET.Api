using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Common.Models.Authentication;

public class GoogleAuthRequestDto
{
    [Required(ErrorMessage = "El token de Google es requerido")]
    public string GoogleIdToken { get; set; } = string.Empty;
}
