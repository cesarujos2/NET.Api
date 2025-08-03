using Microsoft.EntityFrameworkCore;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Interfaces;
using NET.Api.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace NET.Api.Infrastructure.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public EmailTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> FindAsync(Expression<Func<EmailTemplate, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplate> AddAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByTypeAsync(string templateType, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(x => x.TemplateType == templateType && x.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTypeAsync(string templateType, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AnyAsync(x => x.TemplateType == templateType, cancellationToken);
    }
}