# Google Authentication Command

## Overview
This command handles Google authentication using Google ID tokens, seamlessly integrating with the existing JWT authentication system.

## Architecture
- **Command**: `GoogleLoginCommand` - Contains the Google ID token
- **Handler**: `GoogleLoginCommandHandler` - Processes the Google authentication
- **Service**: `GoogleAuthService` - Validates Google tokens and manages user creation/retrieval

## Flow
1. Client sends Google ID token to `/api/authentication/google-login`
2. Token is validated with Google using `Google.Apis.Auth`
3. User is created or retrieved based on Google email
4. JWT tokens are generated using existing `JwtTokenService`
5. Returns `AuthResponseDto` with JWT tokens

## Configuration
Add to `appsettings.json`:
```json
{
  "GoogleAuth": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

## Usage
```csharp
POST /api/authentication/google-login
{
  "googleIdToken": "eyJ..."
}
```

## Response
```json
{
  "success": true,
  "message": "Autenticación con Google exitosa.",
  "data": {
    "id": "user-id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "accessToken": "jwt-access-token",
    "refreshToken": "jwt-refresh-token",
    "expiresAt": "2024-01-01T12:00:00Z",
    "roles": ["User"],
    "requiresEmailConfirmation": false
  }
}
```
