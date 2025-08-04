using FluentValidation;

namespace NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommandValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("El nuevo email es requerido.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .MaximumLength(256).WithMessage("El email no puede exceder los 256 caracteres.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token de confirmación es requerido.");
    }
}