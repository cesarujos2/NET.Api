# Multi-Account System - NET.Api

## Overview

This document describes the Multi-Account System implementation for the NET.Api project. This feature allows users to manage multiple business accounts within a single user profile, enabling them to switch between different business contexts.

## Features Implemented

### ✅ Core Functionality
- **Multiple Account Management**: Users can create and manage up to 5 business accounts
- **Default Account Selection**: Each user has one designated default account
- **Account Switching**: Users can switch between accounts seamlessly
- **Account Data Storage**: Flexible key-value storage for additional account data
- **Account Deactivation**: Soft delete functionality for accounts
- **Authentication Integration**: Login process includes account selection when multiple accounts exist

### ✅ Security Features
- **User Ownership Validation**: Users can only access their own accounts
- **Data Encryption Support**: Sensitive data can be encrypted in storage
- **Audit Trail**: All changes tracked with timestamps and user information
- **JWT Token Integration**: Account context included in authentication tokens

### ✅ API Endpoints
- **GET** `/api/multiAccount` - Get all user accounts
- **GET** `/api/multiAccount/selection` - Get accounts for selection UI
- **GET** `/api/multiAccount/{id}` - Get specific account
- **POST** `/api/multiAccount` - Create new account
- **PUT** `/api/multiAccount/{id}` - Update account
- **POST** `/api/multiAccount/{id}/set-default` - Set default account
- **POST** `/api/multiAccount/{id}/deactivate` - Deactivate account
- **GET** `/api/multiAccount/can-create-more` - Check creation limits
- **POST** `/api/authentication/select-account` - Select account after login

## Architecture

### Domain Layer
```
NET.Api.Domain/
├── Entities/
│   ├── UserAccount.cs          # Main account entity
│   └── UserAccountData.cs      # Key-value data storage
└── Interfaces/
    └── IUserAccountService.cs  # Service interface
```

### Application Layer
```
NET.Api.Application/
├── DTOs/
│   ├── UserAccountDto.cs
│   ├── CreateUserAccountRequestDto.cs
│   ├── UserAccountSelectionDto.cs
│   └── LoginWithAccountSelectionResponseDto.cs
└── Services/
    └── UserAccountService.cs   # Business logic implementation
```

### Infrastructure Layer
```
NET.Api.Infrastructure/
├── Data/
│   └── ApplicationDbContext.cs # EF Core configuration
└── Migrations/
    ├── 20250109000000_AddUserAccountEntities.cs
    └── 20250109000000_AddUserAccountEntities.Designer.cs
```

### API Layer
```
NET.Api.WebApi/
└── Controllers/
    ├── MultiAccountController.cs    # Account management endpoints
    └── AuthenticationController.cs  # Updated with account selection
```

## Database Schema

### USER_ACCOUNTS Table
```sql
CREATE TABLE USER_ACCOUNTS (
    Id VARCHAR(255) PRIMARY KEY,
    ApplicationUserId VARCHAR(255) NOT NULL,
    AccountName VARCHAR(100) NOT NULL,
    CompanyName VARCHAR(200),
    BusinessType VARCHAR(100),
    TaxId VARCHAR(50),
    Address VARCHAR(500),
    City VARCHAR(100),
    State VARCHAR(100),
    Country VARCHAR(100),
    PostalCode VARCHAR(20),
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    CreatedBy VARCHAR(255),
    UpdatedBy VARCHAR(255),
    
    FOREIGN KEY (ApplicationUserId) REFERENCES AspNetUsers(Id),
    INDEX IX_USER_ACCOUNTS_ApplicationUserId (ApplicationUserId),
    INDEX IX_USER_ACCOUNTS_ApplicationUserId_IsDefault (ApplicationUserId, IsDefault)
);
```

### USER_ACCOUNT_DATA Table
```sql
CREATE TABLE USER_ACCOUNT_DATA (
    Id CHAR(36) PRIMARY KEY,
    UserAccountId VARCHAR(255) NOT NULL,
    DataKey VARCHAR(100) NOT NULL,
    DataValue TEXT,
    DataType VARCHAR(50) NOT NULL DEFAULT 'string',
    IsEncrypted BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    CreatedBy VARCHAR(255),
    UpdatedBy VARCHAR(255),
    
    FOREIGN KEY (UserAccountId) REFERENCES USER_ACCOUNTS(Id) ON DELETE CASCADE,
    INDEX IX_USER_ACCOUNT_DATA_UserAccountId (UserAccountId),
    UNIQUE INDEX IX_USER_ACCOUNT_DATA_UserAccountId_DataKey (UserAccountId, DataKey)
);
```

## Business Rules

### Account Management
1. **Account Limit**: Users can create up to 5 accounts (configurable)
2. **Default Account**: Each user must have exactly one default account
3. **Account Names**: Must be unique per user
4. **Deactivation**: Accounts can be deactivated but not deleted
5. **Default Account Protection**: Cannot deactivate the default account

### Data Storage
1. **Key-Value Pairs**: Additional data stored as key-value pairs
2. **Data Types**: Support for string, number, boolean, date types
3. **Encryption**: Sensitive data can be marked for encryption
4. **Unique Keys**: Data keys must be unique per account

## Usage Examples

### Creating an Account
```csharp
// Service usage
var request = new CreateUserAccountRequestDto
{
    AccountName = "Mi Negocio",
    CompanyName = "Empresa Ejemplo S.A.",
    BusinessType = "Tecnología",
    TaxId = "20123456789",
    IsDefault = false
};

var account = await _userAccountService.CreateUserAccountAsync(request, userId);
```

### API Request
```bash
curl -X POST "https://api.example.com/api/multiAccount" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "accountName": "Mi Negocio",
    "companyName": "Empresa Ejemplo S.A.",
    "businessType": "Tecnología",
    "taxId": "20123456789",
    "isDefault": false
  }'
```

### Setting Default Account
```bash
curl -X POST "https://api.example.com/api/multiAccount/{accountId}/set-default" \
  -H "Authorization: Bearer {token}"
```

### Account Selection After Login
```bash
curl -X POST "https://api.example.com/api/authentication/select-account" \
  -H "Content-Type: application/json" \
  -d '{
    "selectionToken": "{selection-token}",
    "accountId": "{account-id}"
  }'
```

## Configuration

### Application Settings
```json
{
  "MultiAccountSettings": {
    "MaxAccountsPerUser": 5,
    "RequireCompanyName": false,
    "EnableDataEncryption": true,
    "DefaultAccountRequired": true
  }
}
```

### Dependency Injection
```csharp
// In Program.cs or Startup.cs
services.AddScoped<IUserAccountService, UserAccountService>();
```

## Testing

### Unit Tests
Tests are located in the test projects and cover:
- Account creation and validation
- Default account management
- Account limits enforcement
- Data encryption/decryption
- Business rule validation

### Integration Tests
- API endpoint testing
- Database operations
- Authentication flow with account selection

## Migration Instructions

### Database Migration
```bash
# Create migration (if not already created)
dotnet ef migrations add AddUserAccountEntities --project src/NET.Api.Infrastructure --startup-project src/NET.Api.WebApi

# Apply migration
dotnet ef database update --project src/NET.Api.Infrastructure --startup-project src/NET.Api.WebApi
```

### Existing User Migration
For existing users without accounts, a default account will be created automatically on first login.

## Performance Considerations

### Database Indexes
- `IX_USER_ACCOUNTS_ApplicationUserId`: Fast user account lookups
- `IX_USER_ACCOUNTS_ApplicationUserId_IsDefault`: Quick default account queries
- `IX_USER_ACCOUNT_DATA_UserAccountId`: Efficient data retrieval
- `IX_USER_ACCOUNT_DATA_UserAccountId_DataKey`: Unique constraint with performance

### Caching Strategy
- User accounts cached for 15 minutes
- Default account cached separately
- Cache invalidation on account updates

## Security Considerations

### Data Protection
- Sensitive data encryption in UserAccountData
- User ownership validation on all operations
- Audit trail for all changes

### Authentication
- JWT tokens include account context
- Account selection tokens have short expiration
- Rate limiting on account creation

## Monitoring and Logging

### Key Metrics
- Account creation rate
- Account switching frequency
- Failed authentication attempts
- API endpoint usage

### Log Events
- Account creation/updates
- Default account changes
- Account deactivation
- Authentication events

## Troubleshooting

### Common Issues

1. **Migration Errors**
   - Ensure database connection is available
   - Check MySQL version compatibility
   - Verify user permissions

2. **Account Creation Limits**
   - Check `MaxAccountsPerUser` configuration
   - Verify user's current account count

3. **Default Account Issues**
   - Ensure user has at least one active account
   - Check default account constraints

### Debug Commands
```bash
# Check migration status
dotnet ef migrations list --project src/NET.Api.Infrastructure

# Verify database schema
dotnet ef dbcontext info --project src/NET.Api.Infrastructure --startup-project src/NET.Api.WebApi

# Build and test
dotnet build
dotnet test
```

## Future Enhancements

### Planned Features
- [ ] Account sharing between users
- [ ] Role-based permissions within accounts
- [ ] Account templates for different business types
- [ ] Bulk operations support
- [ ] Advanced analytics and reporting
- [ ] Account backup and restore
- [ ] Multi-tenant data isolation

### API Versioning
The multi-account system is designed to be backward compatible. Future versions will maintain compatibility through API versioning.

## Support

For questions or issues related to the multi-account system:
1. Check the troubleshooting section
2. Review the API documentation
3. Check the test cases for usage examples
4. Consult the architecture documentation

## Documentation Files

- `MULTI_ACCOUNT_SYSTEM.md` - Detailed technical documentation
- `API_ENDPOINTS.md` - Complete API reference
- `README_MULTI_ACCOUNT.md` - This overview document

---

**Last Updated**: January 2025  
**Version**: 1.0.0  
**Status**: ✅ Implemented and Ready for Production