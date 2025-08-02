using NET.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var userProfile = _mapper.Map<UserProfileDto>(user);
            userProfile.Roles = userRoles.ToList();
            
            _logger.LogInformation("User profile retrieved successfully for user {UserId}", request.UserId);
            
            return userProfile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user {UserId}", request.UserId);
            throw;
        }
    }
}