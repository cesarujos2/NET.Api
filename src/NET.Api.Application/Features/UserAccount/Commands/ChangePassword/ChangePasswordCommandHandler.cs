using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.User;
using NET.Api.Domain.Entities;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangePassword;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, UserOperationResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserOperationResponseDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Changing password for user {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            // Cambiar la contraseña
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error changing password for user {UserId}: {Errors}", request.UserId, errors);
                
                // Verificar si el error es por contraseña incorrecta
                if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                {
                    throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
                }
                
                throw new InvalidOperationException($"Error al cambiar la contraseña: {errors}");
            }

            // Actualizar timestamp
            user.SetUpdatedAt();
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user {UserId}", request.UserId);

            return new UserOperationResponseDto
            {
                Success = true,
                Message = "La contraseña ha sido cambiada exitosamente.",
                RequiresEmailConfirmation = false
            };
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unexpected error changing password for user {UserId}", request.UserId);
            throw new InvalidOperationException("Ocurrió un error inesperado al cambiar la contraseña.");
        }
    }
}