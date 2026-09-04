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