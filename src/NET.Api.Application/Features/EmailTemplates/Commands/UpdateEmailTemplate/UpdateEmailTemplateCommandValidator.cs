using FluentValidation;

namespace NET.Api.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandValidator : AbstractValidator<UpdateEmailTemplateCommand>
{
    public UpdateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del template es requerido.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("El asunto es requerido.")
            .MaximumLength(200).WithMessage("El asunto no puede exceder 200 caracteres.");

        RuleFor(x => x.HtmlContent)
            .NotEmpty().WithMessage("El contenido HTML es requerido.");

        RuleFor(x => x.TextContent)
            .NotEmpty().WithMessage("El contenido de texto es requerido.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}