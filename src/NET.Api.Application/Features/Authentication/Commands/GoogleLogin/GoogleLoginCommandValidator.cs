using FluentValidation;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleLogin;

/// <summary>
/// Validator for GoogleLoginCommand to ensure all required fields are provided
/// and meet security requirements for CSRF protection
/// </summary>
public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("El código de autorización es requerido para la autenticación con Google.");

        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .WithMessage("La URI de redirección es requerida para la autenticación con Google.")
            .Must(BeValidUri)
            .WithMessage("La URI de redirección debe ser una URL válida.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("El parámetro de estado CSRF es requerido para la seguridad de la autenticación.")
            .MinimumLength(10)
            .WithMessage("El parámetro de estado debe tener al menos 10 caracteres para garantizar la seguridad.");
    }

    /// <summary>
    /// Validates that the redirect URI is a valid URL
    /// </summary>
    /// <param name="uri">The URI to validate</param>
    /// <returns>True if the URI is valid, false otherwise</returns>
    private static bool BeValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}