using NET.Api.Domain.Entities;

namespace NET.Api.Domain.Interfaces;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    Task<EmailTemplate?> GetByTypeAsync(string templateType, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByTypeAsync(string templateType, CancellationToken cancellationToken = default);
}