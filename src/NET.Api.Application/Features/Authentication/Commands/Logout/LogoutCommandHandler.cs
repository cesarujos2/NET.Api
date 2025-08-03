using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;

namespace NET.Api.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IAuthService authService, IJwtTokenService jwtTokenService, ILogger<LogoutCommandHandler> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting logout process for user {UserId}", request.UserId);
            
            // Usar el servicio de autenticación para hacer logout
            await _authService.LogoutAsync(request.UserId);
            
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