namespace NET.Api.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, string? textContent = null);
    Task SendEmailConfirmationAsync(string to, string confirmationToken, string userName, string baseUrl);
    Task SendPasswordResetAsync(string to, string resetToken, string userName, string baseUrl);
    Task SendWelcomeEmailAsync(string to, string userName);
}