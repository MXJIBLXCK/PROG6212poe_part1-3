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