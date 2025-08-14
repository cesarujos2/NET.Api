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
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede exceder los 100 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El apellido solo puede contener letras y espacios.");

        RuleFor(x => x.IdentityDocument)
            .NotEmpty().WithMessage("El documento de identidad es requerido.")
            .Length(8, 20).WithMessage("El documento de identidad debe tener entre 8 y 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("El documento de identidad solo puede contener letras y números.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida.")
            .Must(BeAValidAge).WithMessage("Debe ser mayor de 18 años y menor de 120 años.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es requerida.")
            .MinimumLength(10).WithMessage("La dirección debe tener al menos 10 caracteres.")
            .MaximumLength(500).WithMessage("La dirección no puede exceder los 500 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El número de teléfono no puede exceder los 20 caracteres.")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("El formato del número de teléfono no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }

    private static bool BeAValidAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
            
        return age >= 18 && age <= 120;
    }
}