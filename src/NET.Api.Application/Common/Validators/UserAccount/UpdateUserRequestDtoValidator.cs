using FluentValidation;
using NET.Api.Application.Common.Models.User;

namespace NET.Api.Application.Common.Validators.UserAccount;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
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

        RuleFor(x => x.IdentityDocument)
            .NotEmpty().WithMessage("El documento de identidad es requerido.")
            .Length(8, 20).WithMessage("El documento de identidad debe tener entre 8 y 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("El documento de identidad solo puede contener letras y números.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida.")
            .Must(BeAValidAge).WithMessage("Debe ser mayor de 18 años y menor de 120 años.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es requerida.")
            .MaximumLength(200).WithMessage("La dirección no puede exceder los 200 caracteres.");
    }

    private static bool BeAValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18 && age <= 120;
    }
}