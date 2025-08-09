# Google Auth URL Command

## Overview

The `GoogleAuthUrlCommand` generates Google OAuth 2.0 authentication URLs with enhanced CSRF protection using memory cache for state validation.

## Features

### CSRF Protection
- **Automatic State Generation**: Generates unique GUID if state not provided
- **Memory Cache Storage**: Stores state tokens with 10-minute expiration
- **Security Validation**: Enables validation during login process
- **One-time Use**: State tokens are consumed after validation

### URL Generation
- **OAuth 2.0 Compliance**: Generates standard Google OAuth URLs
- **Configurable Parameters**: Supports custom redirect URIs and state
- **Secure Encoding**: Properly encodes all URL parameters

## Command Structure

```csharp
public class GoogleAuthUrlCommand : ICommand<GoogleAuthUrlResponseDto>
{
    public string RedirectUri { get; set; } = string.Empty;
    public string? State { get; set; }
}
```

## Handler Implementation

### State Management Process

1. **State Generation**: Creates unique GUID if state not provided
2. **Cache Storage**: Stores state in memory cache with 10-minute expiration
3. **URL Construction**: Builds Google OAuth URL with all parameters
4. **Response Creation**: Returns URL and state for client use

### Cache Configuration

- **Cache Key Format**: `google_auth_state_{state}`
- **Expiration Time**: 10 minutes
- **Storage Value**: Boolean true (existence check)
- **Cleanup**: Automatic expiration and manual removal after use

## API Usage

### Request

```http
GET /api/authentication/google/auth-url?redirectUri=https://tuapp.com/callback&state=optional_state
```

### Response

```json
{
  "success": true,
  "message": "URL de autenticación con Google obtenida exitosamente.",
  "data": {
    "authUrl": "https://accounts.google.com/o/oauth2/v2/auth?client_id=...",
    "state": "generated_or_provided_state"
  }
}
```

## Parameters

- **redirectUri**: URI where Google redirects after authentication (required)
- **state**: CSRF protection token (optional - auto-generated if not provided)

## Security Features

- **CSRF Prevention**: State parameter prevents cross-site request forgery
- **Token Expiration**: Limited time window for authentication completion
- **Unique Tokens**: Each request generates unique state if not provided
- **Cache Validation**: Server-side validation during login process

## Authentication Flow

1. **URL Request**: Client requests Google authentication URL
2. **State Generation**: Server generates or uses provided state
3. **Cache Storage**: State stored in memory cache for validation
4. **URL Response**: Client receives authentication URL with state
5. **User Redirect**: Client redirects user to Google authentication
6. **Google Callback**: Google redirects back with code and state
7. **State Validation**: Server validates state during login process
8. **Token Exchange**: Code exchanged for tokens after validation

## Dependencies

- `IGoogleAuthService`: Handles Google URL generation
- `IMemoryCache`: Stores and manages CSRF state tokens
- `GoogleAuthUrlResponseDto`: Response model with URL and state

## Related Components

- `GoogleLoginCommand`: Validates state during authentication
- `GoogleAuthService`: Constructs OAuth URLs
- `AuthenticationController`: Exposes HTTP endpoints
