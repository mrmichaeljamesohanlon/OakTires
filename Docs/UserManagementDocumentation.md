---
title: OakTires — User Management API
description: Documentation for the OakTires User Management API (.NET 8). Includes setup, configuration, authentication, webhooks, logging, and troubleshooting.
author: OakTires Dev Team
version: 1.0
last_updated: 2025-10-29
---

# OakTires — User Management API

This document outlines setup, configuration, authentication, webhooks, logging, and data contracts for the **UserManagementApi** built on **.NET 8**.

---

## 🚀 Quick Setup & Run

### Requirements
- .NET 8 SDK  
- SQL Server (local, remote, or container)  
- Git  

### 1. Clone the repository
```bash
git clone <repo-url>
cd OakTires
2. Configure SQL Server
Enable Mixed Mode Authentication in SSMS and restart the SQL Server service.

Enable TCP/IP in SQL Server Configuration Manager and ensure port 1433 is reachable.

Example T-SQL to create database, login, and map user:

sql
Copy code
CREATE DATABASE OakTiresDb;
CREATE LOGIN oak_user WITH PASSWORD = 'Password123!';
USE OakTiresDb;
CREATE USER oak_user FOR LOGIN oak_user;
ALTER ROLE db_owner ADD MEMBER oak_user;
3. Configure appsettings.json
Update the connection string:

json
Copy code
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:127.0.0.1,1433;Database=OakTiresDb;User Id=oak_user;Password=Password123!;"
}
4. Run the application
bash
Copy code
dotnet run
Open Swagger UI:
👉 http://localhost:<port>/swagger

📘 API Overview
Method	Endpoint	Description	Auth Required
POST	/auth/register	Register a new user (RegisterDto)	❌
POST	/auth/login	Login and obtain JWT (LoginDto)	❌
GET	/users	List all users	✅
GET	/users/{id}	Get user by ID	✅
PUT	/users/{id}	Update user	✅
DELETE	/users/{id}	Delete user	✅

Controllers are added via:

csharp
Copy code
builder.Services.AddControllers();
app.MapControllers();
🔐 Authentication (JWT)
Obtain a token via POST /auth/login.

Include the token in the Authorization header for protected endpoints:

makefile
Copy code
Authorization: Bearer <token>
Configuration (appsettings.json):

json
Copy code
"Jwt": {
  "Key": "<secret-key>",
  "Issuer": "OakTiresApi",
  "Audience": "OakTiresClient",
  "ExpireMinutes": 60
}
Tokens are HMAC-SHA256 signed.

🔔 Webhook Behavior
Purpose: Notify external systems on successful login events.

Configuration:

json
Copy code
"Webhook": {
  "LoginEventUrl": "https://example.com/webhooks/login"
}
Triggered asynchronously after successful login (fire-and-forget).

Failures are logged but do not block authentication.

For production:

Secure webhook endpoints (HMAC signature or bearer token).

Use a background queue or retry policy for reliability.

🪵 Logging Overview
Serilog writes to both Console and File.

File sink path can be set in Program.cs.

Example file path:

lua
Copy code
C:\Git\OakTires\Logs\log-YYYYMMDD.txt
Ensure:

The logs directory exists.

The process user has write permission.

Serilog self-log is enabled to capture sink errors.

📦 Data Contracts
User (Database Model)
Property	Type	Description
Id	GUID	Unique user identifier
Username	string	Login username
Email	string	User email
Password	string	BCrypt-hashed password
FirstName	string	First name
LastName	string	Last name
CreatedAt	DateTime (UTC)	Creation timestamp
LastLoginAt	DateTime? (UTC)	Last login timestamp

RegisterDto
Includes fields for username, email, password, first name, and last name.
Passwords are always stored as BCrypt hashes — never in plaintext.

🧩 Troubleshooting
Database connection issues:

Confirm SQL Server is running, Mixed Mode and TCP/IP are enabled, and port 1433 is open.

Test connection:

bash
Copy code
sqlcmd -S tcp:127.0.0.1,1433 -U oak_user -P "Password123!" -Q "SELECT DB_NAME();"
Login failed (Error 18456):

Check SQL Server error log for the state code.

Verify Mixed Mode is enabled and credentials are correct.

Missing logs:

Ensure the log directory exists and is writable.

Check Serilog self-log output in stderr.

🔒 Security Notes
Never commit secrets (JWT keys, DB credentials) to source control.

Use environment variables, User Secrets, or a secrets manager.

Enforce TLS for both database and webhook communications.

In production, prefer managed identities or integrated authentication.

🧭 Next Steps (Optional)
Add a Postman collection with sample requests.

Integrate CI/CD to build and run tests.

Replace the fire-and-forget webhook with a reliable background queue and retry logic.