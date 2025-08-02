using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;

namespace NET.Api.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IJwtTokenService jwtTokenService, ILogger<LogoutCommandHandler> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting logout process for user {UserId}", request.UserId);
            
            // Validar que el token pertenece al usuario
            var tokenUserId = _jwtTokenService.GetUserIdFromToken(request.AccessToken);
            if (tokenUserId != request.UserId)
            {
                _logger.LogWarning("Token mismatch during logout for user {UserId}", request.UserId);
                throw new UnauthorizedAccessException("Token no válido para este usuario.");
            }
            
            // Revocar todos los refresh tokens del usuario
            await _jwtTokenService.RevokeAllRefreshTokensAsync(request.UserId);
            
            _logger.LogInformation("User {UserId} logged out successfully. All refresh tokens revoked.", request.UserId);
            
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // Re-lanzar excepciones de autorización sin logging adicional
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", request.UserId);
            throw new InvalidOperationException("Error al procesar el logout.", ex);
        }
    }
}