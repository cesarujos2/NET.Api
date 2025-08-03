using AutoMapper;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;
using NET.Api.Domain.Interfaces;

namespace NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public class GetEmailTemplatesQueryHandler : IQueryHandler<GetEmailTemplatesQuery, IEnumerable<EmailTemplateDto>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailTemplatesQueryHandler> _logger;

    public GetEmailTemplatesQueryHandler(
        IEmailTemplateRepository emailTemplateRepository,
        IMapper mapper,
        ILogger<GetEmailTemplatesQueryHandler> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EmailTemplateDto>> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var templates = await _emailTemplateRepository.GetAllAsync(cancellationToken);
            var result = _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
            
            _logger.LogInformation("Retrieved {Count} email templates", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email templates");
            throw;
        }
    }
}