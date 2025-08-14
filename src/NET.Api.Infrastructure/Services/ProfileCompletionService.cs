using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;

namespace NET.Api.Infrastructure.Services;

public class ProfileCompletionService : IProfileCompletionService
{
    private readonly Dictionary<string, Func<ApplicationUser, bool>> _requiredFieldValidators;
    private readonly Dictionary<string, string> _fieldDisplayNames;

    public ProfileCompletionService()
    {
        _requiredFieldValidators = new Dictionary<string, Func<ApplicationUser, bool>>
        {
            { nameof(ApplicationUser.FirstName), user => !string.IsNullOrWhiteSpace(user.FirstName) },
            { nameof(ApplicationUser.LastName), user => !string.IsNullOrWhiteSpace(user.LastName) },
            { nameof(ApplicationUser.IdentityDocument), user => !string.IsNullOrWhiteSpace(user.IdentityDocument) },
            { nameof(ApplicationUser.DateOfBirth), user => user.DateOfBirth.HasValue },
            { nameof(ApplicationUser.Address), user => !string.IsNullOrWhiteSpace(user.Address) }
        };

        _fieldDisplayNames = new Dictionary<string, string>
        {
            { nameof(ApplicationUser.FirstName), "Nombre" },
            { nameof(ApplicationUser.LastName), "Apellido" },
            { nameof(ApplicationUser.IdentityDocument), "Documento de identidad" },
            { nameof(ApplicationUser.DateOfBirth), "Fecha de nacimiento" },
            { nameof(ApplicationUser.Address), "Dirección" }
        };
    }

    public bool IsProfileComplete(ApplicationUser user)
    {
        return _requiredFieldValidators.Values.All(validator => validator(user));
    }

    public List<string> GetMissingRequiredFields(ApplicationUser user)
    {
        var missingFields = new List<string>();

        foreach (var validator in _requiredFieldValidators)
        {
            if (!validator.Value(user))
            {
                var displayName = _fieldDisplayNames.TryGetValue(validator.Key, out var name) ? name : validator.Key;
                missingFields.Add(displayName);
            }
        }

        return missingFields;
    }

    public void UpdateProfileCompletionStatus(ApplicationUser user)
    {
        user.IsProfileComplete = IsProfileComplete(user);
        user.SetUpdatedAt();
    }
}