using AutoMapper;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;
using NET.Api.Domain.Interfaces;

namespace NET.Api.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandHandler : ICommandHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmailTemplateCommandHandler> _logger;

    public UpdateEmailTemplateCommandHandler(
        IEmailTemplateRepository emailTemplateRepository,
        IMapper mapper,
        ILogger<UpdateEmailTemplateCommandHandler> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _emailTemplateRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Email template with ID {TemplateId} not found", request.Id);
                throw new InvalidOperationException("Template de email no encontrado.");
            }

            template.UpdateTemplate(
                request.Subject,
                request.HtmlContent,
                request.TextContent,
                request.Description,
                null); // UserId not available in Application layer

            await _emailTemplateRepository.UpdateAsync(template, cancellationToken);
            
            var result = _mapper.Map<EmailTemplateDto>(template);
            
            _logger.LogInformation("Email template {TemplateId} updated successfully", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template {TemplateId}", request.Id);
            throw;
        }
    }
}