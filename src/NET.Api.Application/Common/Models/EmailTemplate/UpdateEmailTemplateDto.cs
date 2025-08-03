namespace NET.Api.Application.Common.Models.EmailTemplate;

public class UpdateEmailTemplateDto
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public string? Description { get; set; }
}