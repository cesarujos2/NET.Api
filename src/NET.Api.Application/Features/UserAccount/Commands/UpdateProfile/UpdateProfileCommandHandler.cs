using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Domain.Entities;

namespace NET.Api.Application.Features.UserAccount.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizando perfil del usuario {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            // Actualizar propiedades del usuario
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.SetUpdatedAt();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error al actualizar el perfil del usuario {UserId}: {Errors}", request.UserId, errors);
                throw new InvalidOperationException($"Error al actualizar el perfil: {errors}");
            }

            _logger.LogInformation("Perfil actualizado exitosamente para el usuario {UserId}", request.UserId);

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            // Mapear a DTO
            var userProfileDto = _mapper.Map<UserProfileDto>(user);
            userProfileDto.Roles = roles.ToList();

            return userProfileDto;
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error inesperado al actualizar el perfil del usuario {UserId}", request.UserId);
            throw new InvalidOperationException("Ocurrió un error inesperado al actualizar el perfil.");
        }
    }
}