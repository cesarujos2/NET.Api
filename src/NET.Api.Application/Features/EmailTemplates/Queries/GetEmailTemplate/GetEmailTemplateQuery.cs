using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;

namespace NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplate;

public class GetEmailTemplateQuery : IQuery<EmailTemplateDto>
{
    public Guid Id { get; set; }
}