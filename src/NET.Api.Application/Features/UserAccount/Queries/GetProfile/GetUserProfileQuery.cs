using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.UserAccount.Queries.GetProfile;

public class GetUserProfileQuery : IQuery<UserProfileDto>
{
    public string UserId { get; set; } = string.Empty;
}