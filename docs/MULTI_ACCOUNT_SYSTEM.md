# Multi-Account System Documentation

## Overview

The Multi-Account System allows users to manage multiple business accounts within a single user profile. This feature enables users to switch between different business contexts, each with its own data and settings.

## Architecture

### Domain Entities

#### UserAccount
Represents a business account associated with a user.

**Properties:**
- `Id` (string): Unique identifier for the account
- `ApplicationUserId` (string): Reference to the user who owns this account
- `AccountName` (string): Display name for the account
- `CompanyName` (string, optional): Company name associated with the account
- `BusinessType` (string, optional): Type of business
- `TaxId` (string, optional): Tax identification number
- `Address` (string, optional): Business address
- `City` (string, optional): City
- `State` (string, optional): State/Province
- `Country` (string, optional): Country
- `PostalCode` (string, optional): Postal/ZIP code
- `IsDefault` (bool): Indicates if this is the default account for the user
- `IsActive` (bool): Indicates if the account is active
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp
- `CreatedBy` (string): User who created the account
- `UpdatedBy` (string): User who last updated the account

#### UserAccountData
Stores additional key-value data for user accounts.

**Properties:**
- `Id` (Guid): Unique identifier
- `UserAccountId` (string): Reference to the UserAccount
- `DataKey` (string): Key for the data entry
- `DataValue` (string): Value for the data entry
- `DataType` (string): Type of data (string, number, boolean, etc.)
- `IsEncrypted` (bool): Indicates if the value is encrypted
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp
- `CreatedBy` (string): User who created the entry
- `UpdatedBy` (string): User who last updated the entry

### Application Layer

#### Services

##### IUserAccountService
Interface for managing user accounts.

**Methods:**
- `GetUserAccountsAsync(string userId)`: Get all accounts for a user
- `GetUserAccountsForSelectionAsync(string userId)`: Get accounts formatted for selection UI
- `GetUserAccountByIdAsync(string accountId)`: Get a specific account by ID
- `CreateUserAccountAsync(CreateUserAccountRequestDto request, string userId)`: Create a new account
- `UpdateUserAccountAsync(string accountId, CreateUserAccountRequestDto request)`: Update an existing account
- `SetDefaultAccountAsync(string accountId, string userId)`: Set an account as default
- `DeactivateUserAccountAsync(string accountId, string userId)`: Deactivate an account
- `CanCreateMoreAccountsAsync(string userId)`: Check if user can create more accounts

#### DTOs

##### UserAccountDto
Data transfer object for user account information.

##### CreateUserAccountRequestDto
Request object for creating/updating user accounts.

##### UserAccountSelectionDto
Simplified DTO for account selection scenarios.

### Infrastructure Layer

#### Database Configuration

**Tables:**
- `USER_ACCOUNTS`: Stores user account information
- `USER_ACCOUNT_DATA`: Stores additional key-value data for accounts

**Indexes:**
- `IX_USER_ACCOUNTS_ApplicationUserId`: Index on user ID
- `IX_USER_ACCOUNTS_ApplicationUserId_IsDefault`: Composite index for finding default accounts
- `IX_USER_ACCOUNT_DATA_UserAccountId`: Index on account ID
- `IX_USER_ACCOUNT_DATA_UserAccountId_DataKey`: Unique composite index for data entries

### API Endpoints

#### MultiAccountController

##### GET /api/multiAccount
Retrieve all user accounts for the authenticated user.

**Response:**
```json
{
  "success": true,
  "message": "Cuentas obtenidas exitosamente.",
  "data": [
    {
      "id": "account-id",
      "accountName": "My Business",
      "companyName": "Example Corp",
      "isDefault": true,
      "isActive": true
    }
  ]
}
```

##### GET /api/multiAccount/selection
Retrieve accounts formatted for selection UI.

##### GET /api/multiAccount/{accountId}
Retrieve a specific user account by ID.

##### POST /api/multiAccount
Create a new user account.

**Request Body:**
```json
{
  "accountName": "My New Business",
  "companyName": "New Corp",
  "businessType": "Technology",
  "taxId": "123456789",
  "address": "123 Main St",
  "city": "City",
  "state": "State",
  "country": "Country",
  "postalCode": "12345",
  "isDefault": false
}
```

##### PUT /api/multiAccount/{accountId}
Update an existing user account.

##### POST /api/multiAccount/{accountId}/set-default
Set an account as the default account for the user.

##### POST /api/multiAccount/{accountId}/deactivate
Deactivate a user account.

##### GET /api/multiAccount/can-create-more
Check if the user can create more accounts.

#### Authentication Updates

##### POST /api/authentication/login
Updated to return `LoginWithAccountSelectionResponseDto` which includes account selection information.

##### POST /api/authentication/google-login
Updated to return `LoginWithAccountSelectionResponseDto` for Google authentication.

##### POST /api/authentication/select-account
New endpoint for selecting an account after login.

**Request Body:**
```json
{
  "selectionToken": "token-from-login",
  "accountId": "selected-account-id"
}
```

## Business Rules

1. **Default Account**: Each user must have exactly one default account.
2. **Account Limits**: Users can create up to 5 accounts (configurable).
3. **Account Ownership**: Users can only access accounts they own.
4. **Deactivation**: Accounts can be deactivated but not deleted to maintain data integrity.
5. **Data Encryption**: Sensitive data in UserAccountData can be encrypted based on the `IsEncrypted` flag.

## Security Considerations

1. **Authorization**: All endpoints require authentication.
2. **Account Ownership**: The system verifies that users can only access their own accounts.
3. **Data Protection**: Sensitive information can be encrypted in the UserAccountData table.
4. **Audit Trail**: All changes are tracked with timestamps and user information.

## Usage Examples

### Creating a New Account
```csharp
var request = new CreateUserAccountRequestDto
{
    AccountName = "My Business",
    CompanyName = "Example Corp",
    BusinessType = "Technology",
    IsDefault = false
};

var account = await userAccountService.CreateUserAccountAsync(request, userId);
```

### Switching Default Account
```csharp
await userAccountService.SetDefaultAccountAsync(accountId, userId);
```

### Retrieving User Accounts
```csharp
var accounts = await userAccountService.GetUserAccountsAsync(userId);
```

## Migration Information

The system includes a database migration `AddUserAccountEntities` that creates the necessary tables and indexes for the multi-account functionality.

**Migration File:** `20250109000000_AddUserAccountEntities.cs`

## Future Enhancements

1. **Account Sharing**: Allow users to share accounts with other users.
2. **Role-based Access**: Implement different permission levels within accounts.
3. **Account Templates**: Provide templates for common business types.
4. **Bulk Operations**: Support bulk operations on multiple accounts.
5. **Account Analytics**: Provide usage analytics for each account.