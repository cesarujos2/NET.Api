using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.User;
using NET.Api.Domain.Entities;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;

public class ChangeEmailCommandHandler : ICommandHandler<ChangeEmailCommand, UserOperationResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ChangeEmailCommandHandler> _logger;

    public ChangeEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ChangeEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<UserOperationResponseDto> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting email change for user {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            // Verificar contraseña actual
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Incorrect password for user {UserId}", request.UserId);
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
            }

            // Verificar que el nuevo email no esté en uso
            var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                _logger.LogWarning("Email {NewEmail} is already in use", request.NewEmail);
                throw new InvalidOperationException("El email ya está en uso por otro usuario.");
            }

            // Generar token de confirmación para el nuevo email
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

            // Enviar email de confirmación
            var confirmationUrl = $"{request.BaseUrl}/confirm-email-change?userId={user.Id}&email={Uri.EscapeDataString(request.NewEmail)}&token={Uri.EscapeDataString(token)}";
            
            await _emailService.SendEmailChangeConfirmationAsync(
                request.NewEmail,
                user.FirstName,
                confirmationUrl);

            _logger.LogInformation("Email change confirmation sent for user {UserId}", request.UserId);

            return new UserOperationResponseDto
            {
                Success = true,
                Message = "Se ha enviado un email de confirmación a la nueva dirección. Por favor, confirma el cambio desde tu email.",
                RequiresEmailConfirmation = true
            };
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error inesperado al cambiar email del usuario {UserId}", request.UserId);
            throw new InvalidOperationException("Ocurrió un error inesperado al cambiar el email.");
        }
    }
}