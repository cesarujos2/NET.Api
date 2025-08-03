using AutoMapper;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;
using NET.Api.Domain.Interfaces;

namespace NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplate;

public class GetEmailTemplateQueryHandler : IQueryHandler<GetEmailTemplateQuery, EmailTemplateDto>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailTemplateQueryHandler> _logger;

    public GetEmailTemplateQueryHandler(
        IEmailTemplateRepository emailTemplateRepository,
        IMapper mapper,
        ILogger<GetEmailTemplateQueryHandler> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto> Handle(GetEmailTemplateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _emailTemplateRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Email template with ID {TemplateId} not found", request.Id);
                throw new InvalidOperationException("Template de email no encontrado.");
            }

            var result = _mapper.Map<EmailTemplateDto>(template);
            
            _logger.LogInformation("Retrieved email template {TemplateId}", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email template {TemplateId}", request.Id);
            throw;
        }
    }
}