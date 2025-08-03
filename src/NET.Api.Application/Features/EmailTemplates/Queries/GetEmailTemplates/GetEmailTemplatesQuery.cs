using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.EmailTemplate;

namespace NET.Api.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public class GetEmailTemplatesQuery : IQuery<IEnumerable<EmailTemplateDto>>
{
}