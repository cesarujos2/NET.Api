using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.User;
using NET.Api.Domain.Entities;

namespace NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommandHandler : ICommandHandler<ConfirmEmailChangeCommand, UserOperationResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailChangeCommandHandler> _logger;

    public ConfirmEmailChangeCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailChangeCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserOperationResponseDto> Handle(ConfirmEmailChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Confirming email change for user {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            // Verificar que el nuevo email no esté en uso por otro usuario
            var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                _logger.LogWarning("Email {NewEmail} is already in use", request.NewEmail);
                throw new InvalidOperationException("El email ya está en uso por otro usuario.");
            }

            // Confirmar el cambio de email
            var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, request.Token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error confirming email change for user {UserId}: {Errors}", request.UserId, errors);
                throw new InvalidOperationException($"Error al confirmar el cambio de email: {errors}");
            }

            // Actualizar el UserName para que coincida con el nuevo email
            user.UserName = request.NewEmail;
            user.SetUpdatedAt();
            
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogError("Error updating UserName for user {UserId}: {Errors}", request.UserId, errors);
                // No lanzamos excepción aquí porque el email ya se cambió exitosamente
            }

            _logger.LogInformation("Email change confirmed successfully for user {UserId}", request.UserId);

            return new UserOperationResponseDto
            {
                Success = true,
                Message = "El email ha sido cambiado exitosamente.",
                RequiresEmailConfirmation = false
            };
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unexpected error confirming email change for user {UserId}", request.UserId);
            throw new InvalidOperationException("Ocurrió un error inesperado al confirmar el cambio de email.");
        }
    }
}