using FluentValidation;
using NET.Api.Shared.Utilities;

namespace NET.Api.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("La URL base es requerida.")
            .Must(UrlValidator.IsValidHttpUrl).WithMessage("La URL base debe ser una URL válida.");
    }
}