# Google Auth URL Command

## Descripción

Este comando se utiliza para obtener la URL de autenticación de Google OAuth 2.0 que los clientes deben usar para iniciar el flujo de autenticación.

## Estructura

- **GoogleAuthUrlCommand**: Comando que contiene los parámetros necesarios
- **GoogleAuthUrlCommandHandler**: Manejador que genera la URL de autenticación
- **GoogleAuthUrlResponseDto**: Respuesta con la URL y el estado generado

## Uso

### Request

```http
GET /api/authentication/google/auth-url?redirectUri=https://tuapp.com/callback&state=random_state
```

### Response

```json
{
  "success": true,
  "message": "URL de autenticación generada exitosamente.",
  "data": {
    "authUrl": "https://accounts.google.com/o/oauth2/v2/auth?client_id=...",
    "state": "random_state"
  }
}
```

## Parámetros

- **redirectUri**: URI a la que Google redirigirá después de la autenticación
- **state**: (opcional) Valor aleatorio para prevenir ataques CSRF

## Flujo de Autenticación

1. Cliente solicita la URL de autenticación
2. Servidor genera la URL con parámetros correctos
3. Cliente redirige al usuario a la URL proporcionada
4. Usuario autentica en Google
5. Google redirige de vuelta con código de autorización
6. Cliente intercambia el código por tokens
