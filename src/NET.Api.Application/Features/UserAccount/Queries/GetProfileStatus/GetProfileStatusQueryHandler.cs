using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.User;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;

namespace NET.Api.Application.Features.UserAccount.Queries.GetProfileStatus;

public class GetProfileStatusQueryHandler : IQueryHandler<GetProfileStatusQuery, UserStatusDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfileCompletionService _profileCompletionService;
    private readonly ILogger<GetProfileStatusQueryHandler> _logger;

    public GetProfileStatusQueryHandler(
        UserManager<ApplicationUser> userManager,
        IProfileCompletionService profileCompletionService,
        ILogger<GetProfileStatusQueryHandler> logger)
    {
        _userManager = userManager;
        _profileCompletionService = profileCompletionService;
        _logger = logger;
    }

    public async Task<UserStatusDto> Handle(GetProfileStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo estado del perfil para el usuario {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            var isComplete = _profileCompletionService.IsProfileComplete(user);
            var missingFields = _profileCompletionService.GetMissingRequiredFields(user);

            var message = isComplete 
                ? "El perfil está completo." 
                : $"Faltan completar los siguientes campos obligatorios: {string.Join(", ", missingFields)}";

            var result = new UserStatusDto
            {
                UserId = user.Id,
                IsProfileComplete = isComplete,
                MissingRequiredFields = missingFields,
                Message = message
            };

            _logger.LogInformation("Estado del perfil obtenido exitosamente para el usuario {UserId}. Completo: {IsComplete}", 
                request.UserId, isComplete);

            return result;
        }
        catch (Exception ex) when (!(ex is NotFoundException))
        {
            _logger.LogError(ex, "Error inesperado al obtener el estado del perfil del usuario {UserId}", request.UserId);
            throw;
        }
    }
}