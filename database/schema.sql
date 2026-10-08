/*
  Reference schema for the Church Hospitality App (SQL Server).

  The recommended way to create the database is EF Core migrations
  (see README.md, "Database setup"), which keep the schema in sync with the code.
  This script documents the two application tables; the ASP.NET Identity tables
  (AspNetUsers, AspNetRoles, AspNetUserRoles, ...) are created by the migration.
*/

IF DB_ID('ChurchHospitalityDb') IS NULL
    CREATE DATABASE ChurchHospitalityDb;
GO

USE ChurchHospitalityDb;
GO

-- AspNetUsers (extended): FullName, ProfilePhotoUrl, IsActive, CreatedAt are the custom columns
-- added on top of the standard Identity columns.

CREATE TABLE Guests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(30) NOT NULL,
    Address NVARCHAR(300) NULL,
    Gender NVARCHAR(20) NULL,
    VisitDate DATETIME2 NOT NULL,
    HowHeardAboutUs NVARCHAR(300) NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE FollowUps (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GuestId INT NOT NULL,
    WeekNumber INT NOT NULL,              -- 1 = week one, 2 = week two
    ScheduledFor DATETIME2 NOT NULL,
    AssignedToUserId NVARCHAR(450) NULL,
    AssignedAt DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 0,        -- 0 Pending, 1 Completed, 2 Missed
    CallDate DATETIME2 NULL,
    Outcome INT NULL,                     -- 0 Reached, 1 NoAnswer, 2 LeftVoicemail, 3 WrongNumber, 4 RequestedNoContact, 5 Other
    Notes NVARCHAR(1500) NULL,
    CompletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_FollowUps_Guests FOREIGN KEY (GuestId) REFERENCES Guests(Id) ON DELETE CASCADE,
    CONSTRAINT FK_FollowUps_AspNetUsers FOREIGN KEY (AssignedToUserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT UQ_FollowUps_Guest_Week UNIQUE (GuestId, WeekNumber)
);

CREATE INDEX IX_FollowUps_AssignedToUserId ON FollowUps(AssignedToUserId);
CREATE INDEX IX_FollowUps_Status ON FollowUps(Status);
GO
