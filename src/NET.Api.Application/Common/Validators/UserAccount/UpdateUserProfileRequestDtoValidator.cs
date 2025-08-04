using FluentValidation;
using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Common.Validators.UserAccount;

public class UpdateUserProfileRequestDtoValidator : AbstractValidator<UpdateUserProfileRequestDto>
{
    public UpdateUserProfileRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("El número de teléfono debe tener un formato válido.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}