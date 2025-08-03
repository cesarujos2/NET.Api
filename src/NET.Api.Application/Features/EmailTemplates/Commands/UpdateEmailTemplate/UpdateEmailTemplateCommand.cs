using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;

namespace NET.Api.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommand : ICommand<EmailTemplateDto>
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public string? Description { get; set; }
}