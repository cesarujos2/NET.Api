using FluentValidation;
using NET.Api.Application.Common.Validators;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;

public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("El nuevo email es requerido.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .MaximumLength(256).WithMessage("El email no puede exceder los 256 caracteres.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("La URL base es requerida.")
            .Matches(@"^https?:\/\/.+").WithMessage("La URL base debe tener un formato válido.");
    }
}