using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Common.Models.EmailTemplate;
using NET.Api.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;
using NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplate;
using NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplates;
using NET.Api.WebApi.Controllers;
using static NET.Api.Shared.Constants.ApiConstants;

namespace NET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.RequireAdminOrAbove)]
public class EmailTemplatesController(IMediator mediator) : BaseApiController
{
    /// <summary>
    /// Get all email templates
    /// </summary>
    /// <returns>List of email templates</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetEmailTemplates()
    {
        var query = new GetEmailTemplatesQuery();
        var result = await mediator.Send(query);
        return Ok(new { success = true, message = "Templates obtenidos exitosamente.", data = result });
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Email template details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailTemplateDto>> GetEmailTemplate(Guid id)
    {
        var query = new GetEmailTemplateQuery { Id = id };
        var result = await mediator.Send(query);
        return Ok(new { success = true, message = "Template obtenido exitosamente.", data = result });
    }

    /// <summary>
    /// Update email template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated email template</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<EmailTemplateDto>> UpdateEmailTemplate(Guid id, [FromBody] UpdateEmailTemplateDto request)
    {
        var command = new UpdateEmailTemplateCommand
        {
            Id = id,
            Subject = request.Subject,
            HtmlContent = request.HtmlContent,
            TextContent = request.TextContent,
            Description = request.Description
        };

        var result = await mediator.Send(command);
        return Ok(new { success = true, message = "Template actualizado exitosamente.", data = result });
    }
}