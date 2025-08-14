namespace NET.Api.Domain.Services;

public interface IProfileCompletionService
{
    bool IsProfileComplete(Entities.ApplicationUser user);
    List<string> GetMissingRequiredFields(Entities.ApplicationUser user);
    void UpdateProfileCompletionStatus(Entities.ApplicationUser user);
}