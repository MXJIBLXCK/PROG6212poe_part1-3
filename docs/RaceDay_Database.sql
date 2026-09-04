-- ****************************************************************************
-- RaceDay Event Management System - Database Script
-- File: docs/RaceDay_Database.sql
-- Target DB: Microsoft SQL Server (SSMS)
-- ****************************************************************************

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END;
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- 1. Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL
);
GO

-- 2. UserProfiles Table
CREATE TABLE UserProfiles (
    ProfileId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NULL,
    EmergencyContactName NVARCHAR(100) NULL,
    EmergencyContactPhone NVARCHAR(20) NULL,
    RunningClub NVARCHAR(100) NULL,
    CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- 3. Events Table
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    EventDate DATETIME2 NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) 
        REFERENCES Users(UserId)
);
GO

-- 4. Categories Table
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL CHECK (DistanceKm > 0),
    EntryFee DECIMAL(10,2) NOT NULL CHECK (EntryFee >= 0),
    MaxParticipants INT NOT NULL CHECK (MaxParticipants > 0),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) 
        REFERENCES Events(EventId) ON DELETE CASCADE
);
GO

-- 5. Enrolments Table
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    ParticipantId INT NOT NULL,
    EnrolmentDate DATETIME2 DEFAULT GETDATE() NOT NULL,
    PaymentStatus NVARCHAR(20) DEFAULT 'Confirmed' NOT NULL 
        CHECK (PaymentStatus IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) 
        REFERENCES Users(UserId),
    CONSTRAINT UQ_Participant_Category UNIQUE (CategoryId, ParticipantId)
);
GO

-- 6. Results Table
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    ParticipantId INT NOT NULL,
    FinishTimeSeconds INT NOT NULL CHECK (FinishTimeSeconds > 0),
    Position INT NULL,
    RecordedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT FK_Results_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Results_Users FOREIGN KEY (ParticipantId) 
        REFERENCES Users(UserId),
    CONSTRAINT UQ_Result_Participant UNIQUE (CategoryId, ParticipantId)
);
GO

-- Seed 1: Users (2 Organisers, 3 Participants)
INSERT INTO Users (Email, PasswordHash, FullName, Role) VALUES
('organiser1@raceday.co.za', 'hashed_pwd_1', 'Nick Fury', 'Organiser'),
('organiser2@raceday.co.za', 'hashed_pwd_2', 'Phillip Coulson', 'Organiser'),
('participant1@gmail.com', 'hashed_pwd_3', 'Tony Stark', 'Participant'),
('participant2@yahoo.com', 'hashed_pwd_4', 'Thor Odinsson', 'Participant'),
('participant3@outlook.com', 'hashed_pwd_5', 'Steve Rogers', 'Participant');
GO

-- Seed 2: UserProfiles
INSERT INTO UserProfiles (UserId, PhoneNumber, EmergencyContactName, EmergencyContactPhone, RunningClub) VALUES
(3, '0821234567', 'Pepper Potts', '0829876543', 'Soweto Striders'),
(4, '0712345678', 'Loki Odinsson', '0719876543', 'Pretoria Athletics Club'),
(5, '0833456789', 'Peggy Carter', '0838765432', 'Kempton Park Striders');
GO