# Authentication Controller

This controller handles all authentication-related endpoints for the application, including traditional email/password authentication and Google OAuth 2.0 integration.

## Google Authentication Flow

The application supports Google OAuth 2.0 authentication with the following flow:

1. **Get Google Authentication URL**
   - Frontend requests a Google authentication URL from the backend
   - Backend generates and returns the URL with proper configuration
   - Frontend redirects the user to the Google authentication page

2. **Handle Google Callback**
   - After user authentication, Google redirects back to the frontend with an authorization code
   - Frontend sends the authorization code to the backend for token exchange
   - Backend exchanges the code for Google ID token and authenticates the user
   - Backend returns JWT tokens for API access

## Endpoints

### 1. Get Google Authentication URL

```http
GET /api/authentication/google/auth-url
```

#### Query Parameters
- `redirectUri`: The URI where Google should redirect after authentication (required)
- `state`: Optional state parameter for CSRF protection

#### Response
```json
{
  "success": true,
  "data": {
    "authUrl": "https://accounts.google.com/o/oauth2/v2/auth?client_id=..."
  }
}
```

### 2. Exchange Google Authorization Code

```http
POST /api/authentication/google/exchange-code
```

#### Request Body
```json
{
  "code": "authorization_code_from_google",
  "redirectUri": "same_redirect_uri_used_in_auth_url"
}
```

#### Response
```json
{
  "success": true,
  "message": "Autenticación con Google exitosa.",
  "data": {
    "id": "user_id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "accessToken": "jwt_access_token",
    "refreshToken": "jwt_refresh_token",
    "expiresAt": "2025-08-08T20:00:00Z",
    "roles": ["User"],
    "requiresEmailConfirmation": false
  }
}
```

### 3. Login with Google ID Token

```http
POST /api/authentication/google/login
```

#### Request Body
```json
{
  "idToken": "google_id_token"
}
```

#### Response
Same as the exchange code endpoint.

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Error message describing the issue"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Error interno del servidor.",
  "details": "Detailed error message"
}
```

## Security Considerations

- Always use HTTPS in production
- Validate and sanitize all user inputs
- Implement proper CORS policies
- Store sensitive configuration (Client ID, Client Secret) in a secure manner
- Use state parameter to prevent CSRF attacks
- Implement rate limiting on authentication endpoints
