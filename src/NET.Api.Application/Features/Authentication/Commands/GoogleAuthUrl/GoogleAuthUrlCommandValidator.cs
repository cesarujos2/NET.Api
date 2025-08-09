using FluentValidation;
using NET.Api.Shared.Utilities;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleAuthUrl;

public class GoogleAuthUrlCommandValidator: AbstractValidator<GoogleAuthUrlCommand>
{
    public GoogleAuthUrlCommandValidator()
    {
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .WithMessage("La URI de redirección es requerida para la autenticación con Google.")
            .Must(UrlValidator.IsValidHttpUrl)
            .WithMessage("La URI de redirección debe ser una URL válida.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("El parámetro de estado CSRF es requerido para la seguridad de la autenticación.")
            .MinimumLength(10)
            .WithMessage("El parámetro de estado debe tener al menos 10 caracteres para garantizar la seguridad.");
    }
}
