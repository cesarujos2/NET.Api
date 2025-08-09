# Google Login Command

## Overview

The `GoogleLoginCommand` handles Google OAuth 2.0 authentication with enhanced CSRF protection and proper Identity integration.

## Features

### CSRF Protection
- **State Parameter Validation**: Validates the CSRF state parameter using `IMemoryCache`
- **Cache-based Security**: Stores state tokens with 10-minute expiration
- **One-time Use**: State tokens are removed after validation to prevent reuse
- **Security Exception Handling**: Throws `UnauthorizedException` for invalid or missing state

### Identity Integration
- **External Login Tracking**: Uses `IdentityUserLogin` table to track Google authentication
- **Provider Information**: Stores Google as external authentication provider
- **User Linking**: Links Google accounts to existing users or creates new ones

## Command Structure

```csharp
public class GoogleLoginCommand : ICommand<AuthResponseDto>
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // CSRF protection
}
```

## Properties

- **Code**: The authorization code received from Google after user authentication
- **RedirectUri**: The redirect URI that was used in the initial authorization request
- **State**: CSRF state parameter for security validation (required)

## Handler Implementation

### CSRF Validation Process

1. **State Parameter Check**: Validates that state parameter is provided
2. **Cache Lookup**: Checks if state exists in memory cache
3. **State Removal**: Removes state from cache to prevent reuse
4. **Exception Handling**: Throws security exceptions for invalid states

### Authentication Flow

1. **CSRF Validation**: Validates state parameter against cache
2. **Code Exchange**: Exchanges authorization code for ID token
3. **Token Validation**: Validates Google ID token
4. **User Management**: Creates or updates user account
5. **External Login Registration**: Records Google login in Identity tables
6. **JWT Generation**: Generates access and refresh tokens

## Security Considerations

- **CSRF Protection**: State parameter prevents cross-site request forgery
- **Token Expiration**: Cache entries expire after 10 minutes
- **One-time Use**: State tokens cannot be reused
- **Provider Validation**: Google ID tokens are validated using official libraries
- **External Login Tracking**: Maintains audit trail of external authentications

## Error Handling

- **Missing State**: "Estado CSRF requerido para la autenticación con Google."
- **Invalid State**: "Estado CSRF inválido o expirado."
- **Token Validation**: Standard Google authentication errors
- **User Creation**: Identity-related errors during user management

## Dependencies

- `IAuthService`: Handles Google authentication logic
- `IMemoryCache`: Stores and validates CSRF state tokens
- `UnauthorizedException`: Security exception for CSRF violations

## Usage Example

```csharp
var command = new GoogleLoginCommand
{
    Code = "authorization_code_from_google",
    RedirectUri = "https://myapp.com/auth/callback",
    State = "csrf_state_token"
};

var result = await mediator.Send(command);
```

## Related Components

- `GoogleAuthUrlCommand`: Generates authentication URL with state
- `GoogleAuthService`: Handles Google API integration
- `AuthenticationController`: Exposes HTTP endpoints
- `IdentityUserLogin`: Tracks external authentication providers
```
