using FluentValidation;

namespace NET.Api.Application.Features.UserAccount.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede exceder los 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El número de teléfono no puede exceder los 20 caracteres.")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("El formato del número de teléfono no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}