using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using NET.Api.Domain.Interfaces;
using NET.Api.Domain.ValueObjects;
using NET.Api.Shared.Constants;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Services;

namespace NET.Api.Infrastructure.Services;

public class EmailService(
    IOptions<SmtpConfiguration> smtpConfig,
    IEmailTemplateRepository emailTemplateRepository,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpConfiguration _smtpConfig = smtpConfig.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlContent, string? textContent = null)
    {
        try
        {
            using var client = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port)
            {
                Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password),
                EnableSsl = _smtpConfig.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpConfig.From),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            if (!string.IsNullOrEmpty(textContent))
            {
                var textView = AlternateView.CreateAlternateViewFromString(textContent, null, "text/plain");
                mailMessage.AlternateViews.Add(textView);
            }

            await client.SendMailAsync(mailMessage);
            logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationToken, string userName, string baseUrl)
    {
        var template = await emailTemplateRepository.GetByTypeAsync(EmailTemplateTypes.EmailConfirmation);
        if (template == null)
        {
            logger.LogError("Email confirmation template not found");
            throw new InvalidOperationException("Email confirmation template not found");
        }

        var confirmationUrl = $"{baseUrl.TrimEnd('/')}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&email={Uri.EscapeDataString(to)}";
        
        var htmlContent = template.HtmlContent
            .Replace("{{UserName}}", userName)
            .Replace("{{ConfirmationUrl}}", confirmationUrl);
            
        var textContent = template.TextContent
            .Replace("{{UserName}}", userName)
            .Replace("{{ConfirmationUrl}}", confirmationUrl);

        var subject = template.Subject.Replace("{{UserName}}", userName);

        await SendEmailAsync(to, subject, htmlContent, textContent);
    }

    public async Task SendPasswordResetAsync(string to, string resetToken, string userName, string baseUrl)
    {
        var template = await emailTemplateRepository.GetByTypeAsync(EmailTemplateTypes.PasswordReset);
        if (template == null)
        {
            logger.LogError("Password reset template not found");
            throw new InvalidOperationException("Password reset template not found");
        }

        var resetUrl = $"{baseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(to)}";
        
        var htmlContent = template.HtmlContent
            .Replace("{{UserName}}", userName)
            .Replace("{{ResetUrl}}", resetUrl);
            
        var textContent = template.TextContent
            .Replace("{{UserName}}", userName)
            .Replace("{{ResetUrl}}", resetUrl);

        var subject = template.Subject.Replace("{{UserName}}", userName);

        await SendEmailAsync(to, subject, htmlContent, textContent);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var template = await emailTemplateRepository.GetByTypeAsync(EmailTemplateTypes.Welcome);
        if (template == null)
        {
            logger.LogError("Welcome email template not found");
            throw new InvalidOperationException("Welcome email template not found");
        }

        var htmlContent = template.HtmlContent.Replace("{{UserName}}", userName);
        var textContent = template.TextContent.Replace("{{UserName}}", userName);
        var subject = template.Subject.Replace("{{UserName}}", userName);

        await SendEmailAsync(to, subject, htmlContent, textContent);
    }
}