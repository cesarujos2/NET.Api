# API Endpoints Documentation

## Multi-Account Management

### Base URL
```
/api/multiAccount
```

### Authentication
All endpoints require Bearer token authentication.

---

## Endpoints

### 1. Get User Accounts

**GET** `/api/multiAccount`

Retrieve all user accounts for the authenticated user.

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Cuentas obtenidas exitosamente.",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "accountName": "Mi Negocio Principal",
      "companyName": "Empresa Ejemplo S.A.",
      "businessType": "Tecnología",
      "taxId": "20123456789",
      "address": "Av. Principal 123",
      "city": "Lima",
      "state": "Lima",
      "country": "Perú",
      "postalCode": "15001",
      "isDefault": true,
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

#### Status Codes
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `500 Internal Server Error`: Server error

---

### 2. Get Accounts for Selection

**GET** `/api/multiAccount/selection`

Retrieve accounts formatted for selection UI (simplified data).

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Cuentas para selección obtenidas exitosamente.",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "accountName": "Mi Negocio Principal",
      "companyName": "Empresa Ejemplo S.A.",
      "isDefault": true
    }
  ]
}
```

---

### 3. Get Account by ID

**GET** `/api/multiAccount/{accountId}`

Retrieve a specific user account by ID.

#### Parameters
- `accountId` (string, required): The unique identifier of the account

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Cuenta obtenida exitosamente.",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "accountName": "Mi Negocio Principal",
    "companyName": "Empresa Ejemplo S.A.",
    "businessType": "Tecnología",
    "taxId": "20123456789",
    "address": "Av. Principal 123",
    "city": "Lima",
    "state": "Lima",
    "country": "Perú",
    "postalCode": "15001",
    "isDefault": true,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:45:00Z"
  }
}
```

#### Status Codes
- `200 OK`: Success
- `404 Not Found`: Account not found or not owned by user
- `401 Unauthorized`: Invalid or missing token

---

### 4. Create User Account

**POST** `/api/multiAccount`

Create a new user account.

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Request Body
```json
{
  "accountName": "Mi Nuevo Negocio",
  "companyName": "Nueva Empresa S.A.C.",
  "businessType": "Comercio",
  "taxId": "20987654321",
  "address": "Jr. Comercio 456",
  "city": "Arequipa",
  "state": "Arequipa",
  "country": "Perú",
  "postalCode": "04001",
  "isDefault": false
}
```

#### Field Validation
- `accountName`: Required, max 100 characters
- `companyName`: Optional, max 200 characters
- `businessType`: Optional, max 100 characters
- `taxId`: Optional, max 50 characters
- `address`: Optional, max 500 characters
- `city`: Optional, max 100 characters
- `state`: Optional, max 100 characters
- `country`: Optional, max 100 characters
- `postalCode`: Optional, max 20 characters
- `isDefault`: Boolean, defaults to false

#### Response
```json
{
  "success": true,
  "message": "Cuenta creada exitosamente.",
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "accountName": "Mi Nuevo Negocio",
    "companyName": "Nueva Empresa S.A.C.",
    "businessType": "Comercio",
    "taxId": "20987654321",
    "address": "Jr. Comercio 456",
    "city": "Arequipa",
    "state": "Arequipa",
    "country": "Perú",
    "postalCode": "04001",
    "isDefault": false,
    "isActive": true,
    "createdAt": "2024-01-25T09:15:00Z",
    "updatedAt": null
  }
}
```

#### Status Codes
- `201 Created`: Account created successfully
- `400 Bad Request`: Validation errors or account limit reached
- `401 Unauthorized`: Invalid or missing token
- `409 Conflict`: Account name already exists for user

---

### 5. Update User Account

**PUT** `/api/multiAccount/{accountId}`

Update an existing user account.

#### Parameters
- `accountId` (string, required): The unique identifier of the account

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Request Body
```json
{
  "accountName": "Mi Negocio Actualizado",
  "companyName": "Empresa Actualizada S.A.",
  "businessType": "Tecnología y Servicios",
  "taxId": "20123456789",
  "address": "Av. Principal 789",
  "city": "Lima",
  "state": "Lima",
  "country": "Perú",
  "postalCode": "15002",
  "isDefault": true
}
```

#### Response
```json
{
  "success": true,
  "message": "Cuenta actualizada exitosamente.",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "accountName": "Mi Negocio Actualizado",
    "companyName": "Empresa Actualizada S.A.",
    "businessType": "Tecnología y Servicios",
    "taxId": "20123456789",
    "address": "Av. Principal 789",
    "city": "Lima",
    "state": "Lima",
    "country": "Perú",
    "postalCode": "15002",
    "isDefault": true,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-25T16:20:00Z"
  }
}
```

#### Status Codes
- `200 OK`: Account updated successfully
- `400 Bad Request`: Validation errors
- `404 Not Found`: Account not found or not owned by user
- `401 Unauthorized`: Invalid or missing token

---

### 6. Set Default Account

**POST** `/api/multiAccount/{accountId}/set-default`

Set an account as the default account for the user.

#### Parameters
- `accountId` (string, required): The unique identifier of the account

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Cuenta establecida como predeterminada exitosamente.",
  "data": null
}
```

#### Status Codes
- `200 OK`: Default account set successfully
- `404 Not Found`: Account not found or not owned by user
- `401 Unauthorized`: Invalid or missing token

---

### 7. Deactivate Account

**POST** `/api/multiAccount/{accountId}/deactivate`

Deactivate a user account (soft delete).

#### Parameters
- `accountId` (string, required): The unique identifier of the account

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Cuenta desactivada exitosamente.",
  "data": null
}
```

#### Status Codes
- `200 OK`: Account deactivated successfully
- `400 Bad Request`: Cannot deactivate default account
- `404 Not Found`: Account not found or not owned by user
- `401 Unauthorized`: Invalid or missing token

---

### 8. Check Account Creation Limit

**GET** `/api/multiAccount/can-create-more`

Check if the user can create more accounts.

#### Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

#### Response
```json
{
  "success": true,
  "message": "Verificación de límite completada.",
  "data": {
    "canCreateMore": true,
    "currentCount": 2,
    "maxAllowed": 5
  }
}
```

#### Status Codes
- `200 OK`: Check completed successfully
- `401 Unauthorized`: Invalid or missing token

---

## Authentication Endpoints (Updated)

### Account Selection After Login

**POST** `/api/authentication/select-account`

Select an account after login when multiple accounts are available.

#### Headers
```
Content-Type: application/json
```

#### Request Body
```json
{
  "selectionToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accountId": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### Response
```json
{
  "success": true,
  "message": "Cuenta seleccionada exitosamente.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresAt": "2024-01-25T18:30:00Z",
    "user": {
      "id": "user-id",
      "email": "user@example.com",
      "firstName": "Juan",
      "lastName": "Pérez"
    },
    "selectedAccount": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "accountName": "Mi Negocio Principal",
      "companyName": "Empresa Ejemplo S.A."
    }
  }
}
```

#### Status Codes
- `200 OK`: Account selected successfully
- `400 Bad Request`: Invalid selection token or account ID
- `404 Not Found`: Account not found or not owned by user

---

## Error Responses

All endpoints may return the following error format:

```json
{
  "success": false,
  "message": "Descripción del error",
  "errors": [
    {
      "field": "accountName",
      "message": "El nombre de la cuenta es requerido."
    }
  ],
  "data": null
}
```

### Common Error Codes
- `400 Bad Request`: Validation errors, business rule violations
- `401 Unauthorized`: Authentication required or invalid token
- `403 Forbidden`: Access denied
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource already exists
- `500 Internal Server Error`: Server error

---

## Rate Limiting

API endpoints are subject to rate limiting:
- **Standard endpoints**: 100 requests per minute per user
- **Account creation**: 5 requests per hour per user
- **Authentication endpoints**: 10 requests per minute per IP

---

## Pagination

For endpoints that return lists, pagination parameters can be used:

```
GET /api/multiAccount?page=1&pageSize=10&sortBy=accountName&sortOrder=asc
```

#### Parameters
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: Field to sort by
- `sortOrder`: Sort direction (asc/desc)

#### Paginated Response
```json
{
  "success": true,
  "message": "Cuentas obtenidas exitosamente.",
  "data": {
    "items": [...],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```