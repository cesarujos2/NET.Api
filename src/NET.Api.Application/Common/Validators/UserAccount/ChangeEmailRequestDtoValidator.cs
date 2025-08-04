using FluentValidation;
using NET.Api.Application.Common.Models.UserAccount;
using NET.Api.Shared.Utilities;

namespace NET.Api.Application.Common.Validators.UserAccount;

public class ChangeEmailRequestDtoValidator : AbstractValidator<ChangeEmailRequestDto>
{
    public ChangeEmailRequestDtoValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("El nuevo email es requerido.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("La URL base es requerida.")
            .Must(UrlValidator.IsValidHttpUrl).WithMessage("La URL base debe ser una URL válida.");
    }
}