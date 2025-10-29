-- Create the Users table
CREATE TABLE dbo.Users
(
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2 NULL
);
GO

-- Optional: Add an index for faster lookups by email or username
CREATE INDEX IX_Users_Email ON dbo.Users (Email);
CREATE INDEX IX_Users_Username ON dbo.Users (Username);
GO