# User Management API (.NET 8)

A secure RESTful API for managing users, built with ASP.NET Core, Entity Framework Core, and JWT authentication.

## Setup & Run Instructions

1. Install .NET 8 SDK and SQL Server
2. Clone the repository
3. Configure your connection string in appsettings.json
4. Run migrations:
   dotnet ef migrations add InitialCreate
   dotnet ef database update
5. Start the API:
   dotnet run
6. Open Swagger UI at:
   http://localhost:5163/swagger

## API Overview & Authentication

### Public Endpoints
- POST /auth/register — Register a new user
- POST /auth/login — Authenticate and receive JWT token

### Protected Endpoints (require JWT)
- GET /users — List all users
- GET /users/{id} — Get user by ID
- PUT /users/{id} — Update user
- DELETE /users/{id} — Delete user

Include the token in the Authorization header:
Authorization: Bearer <your_token>

## Webhook Behavior

On successful login, a webhook is triggered.

Configure the webhook URL in appsettings.json:
"Webhook": {
  "LoginEventUrl": "https://webhook.site/your-unique-id"
}

Payload example:
{
  "userId": "GUID",
  "username": "testuser",
  "timestamp": "2025-10-29T17:53:00Z"
}

## Logging Overview

- Uses ILogger for structured logging
- Logs registration, login, updates, deletions
- Console output by default

## Data Contracts

RegisterDto:
- Username
- Email
- Password
- FirstName
- LastName

LoginDto:
- Username
- Password

UserDto:
- Id
- Username
- Email
- FirstName
- LastName
- CreatedAt
- LastLoginAt

UpdateUserDto:
- Email
- FirstName
- LastName

## Testing

Use Swagger or Postman to test endpoints.

PowerShell test script available at:
Test/testUserApp.ps1

Run it with:
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\Test\testUserApp.ps1

## Contact

Michael O'Hanlon  
m.ohanlon@live.co.uk
