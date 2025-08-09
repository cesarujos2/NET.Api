# Google Authentication Integration Guide

## Overview
This guide documents the complete integration of Google authentication into the existing JWT-based RESTful API, following Clean Architecture principles and providing Spanish API responses.

## Architecture Overview

### Clean Architecture Layers
- **Domain**: Core entities and business rules
- **Application**: Use cases and application logic
- **Infrastructure**: External services and data access
- **WebApi**: Controllers and API endpoints

### Key Components

#### 1. Domain Layer
- **ApplicationUser**: Extended IdentityUser with additional properties
- **ApplicationRole**: Custom role entity

#### 2. Application Layer
- **GoogleAuthRequestDto**: Request model for Google authentication
- **GoogleLoginCommand**: MediatR command for Google login
- **GoogleLoginCommandHandler**: Command handler implementation
- **IGoogleAuthService**: Interface for Google authentication service

#### 3. Infrastructure Layer
- **GoogleAuthService**: Service for validating Google tokens and managing users with proper external login handling
- **GoogleAuthSettings**: Configuration for Google OAuth
- **AuthService**: Extended to include GoogleLoginAsync method

#### 4. WebApi Layer
- **AuthenticationController**: Added GoogleLogin endpoint

## Configuration

### 1. Google Cloud Console Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Configure authorized redirect URIs
6. Copy the Client ID

### 2. Application Configuration
Add the following to `appsettings.json`:

```json
{
  "GoogleAuth": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

### 3. Dependencies
The following NuGet packages are required:
- `Google.Apis.Auth` (1.68.0) - For Google token validation

## API Endpoints

### Google Login Endpoint
```
POST /api/authentication/google-login
```

**Request Body:**
```json
{
  "googleIdToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Autenticación con Google exitosa.",
  "data": {
    "id": "user-guid",
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

**Error Response (400/401):**
```json
{
  "success": false,
  "message": "Token de Google inválido o expirado.",
  "errors": ["El token proporcionado no es válido"]
}
```

## Implementation Details

### User Flow
1. Client obtains Google ID token from Google Sign-In
2. Client sends token to `/api/authentication/google-login`
3. Server validates token with Google
4. Server creates or retrieves user based on Google email
5. Server generates JWT tokens
6. Server returns JWT tokens to client

### User Creation Logic
- **New Users**: Automatically created with Google profile information
- **Existing Users**: Matched by email address and updated with latest Google info
- **Email Confirmation**: Automatically set to confirmed for Google users
- **Default Role**: All new Google users get "User" role
- **Identity Document**: Left empty for Google users (can be updated later)

### Security Considerations
- Google ID tokens are validated using Google's official libraries
- Tokens are verified for audience, issuer, and expiration
- User emails are confirmed automatically (trusted Google verification)
- JWT tokens follow the same security model as regular authentication

## Testing

### Unit Tests
- Test Google token validation
- Test user creation/update logic
- Test JWT token generation
- Test error handling

### Integration Tests
- Test complete Google login flow
- Test with valid and invalid Google tokens
- Test user persistence in database
- Test JWT token validation

### Manual Testing
1. Configure Google OAuth client ID
2. Use Google Sign-In on frontend
3. Send ID token to backend
4. Verify JWT tokens are returned
5. Test protected endpoints with JWT

## Frontend Integration

### Google Sign-In Setup
```javascript
// Initialize Google Sign-In
gapi.load('auth2', function() {
  gapi.auth2.init({
    client_id: 'your-google-client-id.apps.googleusercontent.com'
  });
});

// Get ID token
const googleUser = await gapi.auth2.getAuthInstance().signIn();
const idToken = googleUser.getAuthResponse().id_token;

// Send to backend
const response = await fetch('/api/authentication/google-login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({ googleIdToken: idToken })
});
```

## Error Handling

### Common Error Scenarios
- **Invalid Google Token**: Returns 401 with Spanish message
- **Expired Google Token**: Returns 401 with Spanish message
- **Network Issues**: Returns 500 with generic Spanish error
- **Database Issues**: Returns 500 with generic Spanish error

### Error Messages (Spanish)
- "El token de Google es requerido"
- "Token de Google inválido o expirado"
- "Error al procesar la autenticación con Google"
- "El usuario ya existe con este correo electrónico"

## Migration Notes

### Existing Users
- Users with existing email addresses will be linked to their Google account
- No duplicate accounts are created
- Existing user data is preserved

### Database Schema
- No schema changes required
- Uses existing Identity tables
- Google users get email confirmed automatically

## Monitoring and Logging

### Key Metrics
- Google authentication success/failure rates
- User creation vs. login counts
- Token validation performance
- Error rates and types

### Logging
- All Google authentication attempts are logged
- User creation events are logged
- Token validation failures are logged
- Security events are logged with appropriate levels

## External Login Integration

### ExternalLoginSignInAsync Implementation
The service now uses ASP.NET Core Identity's `ExternalLoginSignInAsync` method for proper external authentication handling:

```csharp
// Try external login sign in first
var signInResult = await _signInManager.ExternalLoginSignInAsync(
    "Google", payload.Subject, isPersistent: false, bypassTwoFactor: false);
```

### Benefits of ExternalLoginSignInAsync
- **Integrated Security**: Leverages Identity's built-in security features
- **Lockout Handling**: Automatically handles account lockouts
- **Two-Factor Support**: Ready for 2FA implementation
- **Consistent Flow**: Uses same authentication pipeline as other providers
- **Policy Enforcement**: Applies all configured Identity policies

### Authentication Flow
1. **Primary Attempt**: Try external login with existing registration
2. **Registration Check**: If failed, verify if external login exists
3. **Auto-Registration**: Add external login if missing
4. **Retry**: Attempt external login again after registration
5. **Fallback**: Use regular sign-in if external login fails
6. **Security Validation**: Check for lockouts and security policies

### Error Handling
- **Account Lockout**: Throws specific exception for locked accounts
- **Registration Failures**: Logs warnings for external login registration issues
- **Graceful Degradation**: Falls back to regular sign-in when needed

## Troubleshooting

### Common Issues
1. **"Invalid Google Client ID"**
   - Verify GoogleAuth:ClientId in appsettings.json
   - Ensure the client ID matches Google Cloud Console

2. **"Token validation failed"**
   - Check Google token expiration
   - Verify token audience matches client ID

3. **"User creation failed"**
   - Check database connectivity
   - Verify Identity configuration

4. **"JWT token not generated"**
   - Check JwtSettings configuration
   - Verify JwtTokenService is properly configured

## Future Enhancements

### Potential Improvements
- Support for multiple Google accounts per user
- Account linking (connect Google to existing account)
- Google refresh token support for offline access
- Social login analytics and reporting
- Advanced user profile synchronization

## Support

For issues or questions about Google authentication integration, please refer to:
- Google OAuth 2.0 documentation
- ASP.NET Core Identity documentation
- Project issue tracker
- Google Cloud Console support
