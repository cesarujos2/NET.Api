using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplates;
using NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplate;
using NET.Api.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;
using NET.Api.Application.Common.Models.EmailTemplate;

namespace NET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTemplatesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all email templates
    /// </summary>
    /// <returns>List of email templates</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetEmailTemplates()
    {
        try
        {
            var query = new GetEmailTemplatesQuery();
            var result = await mediator.Send(query);
            return Ok(new { success = true, message = "Templates obtenidos exitosamente.", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Email template details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailTemplateDto>> GetEmailTemplate(Guid id)
    {
        try
        {
            var query = new GetEmailTemplateQuery { Id = id };
            var result = await mediator.Send(query);
            return Ok(new { success = true, message = "Template obtenido exitosamente.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
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
        try
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
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }
}