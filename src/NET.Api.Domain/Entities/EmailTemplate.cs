namespace NET.Api.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string HtmlContent { get; private set; } = string.Empty;
    public string TextContent { get; private set; } = string.Empty;
    public string TemplateType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }
    
    // Constructor for EF Core
    private EmailTemplate() { }
    
    public EmailTemplate(
        string name,
        string subject,
        string htmlContent,
        string textContent,
        string templateType,
        string? description = null,
        string? createdBy = null)
    {
        Name = name;
        Subject = subject;
        HtmlContent = htmlContent;
        TextContent = textContent;
        TemplateType = templateType;
        Description = description;
        SetCreatedBy(createdBy);
    }
    
    public void UpdateTemplate(
        string subject,
        string htmlContent,
        string textContent,
        string? description = null,
        string? updatedBy = null)
    {
        Subject = subject;
        HtmlContent = htmlContent;
        TextContent = textContent;
        Description = description;
        SetUpdatedAt(updatedBy);
    }
    
    public void Activate(string? updatedBy = null)
    {
        IsActive = true;
        SetUpdatedAt(updatedBy);
    }
    
    public void Deactivate(string? updatedBy = null)
    {
    	IsActive = false;
        SetUpdatedAt(updatedBy);
    }
}