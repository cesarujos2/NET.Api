using FluentValidation;
using NET.Api.Application.Common.Models.User;

namespace NET.Api.Application.Common.Validators.UserAccount;

public class ConfirmUserEmailChangeRequestDtoValidator : AbstractValidator<ConfirmUserEmailChangeRequestDto>
{
    public ConfirmUserEmailChangeRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido.");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("El nuevo email es requerido.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token de confirmación es requerido.");
    }
}