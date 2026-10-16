# RaceDay - Event Management System (POE Part 2)

**Course Code:** PROG6212  
**Portfolio of Evidence (POE) - Part 2**

## Overview
RaceDay is a secure, robust backend Web API built for the South African road running, walking, and cycling community. Developed using **.NET 8** and **Entity Framework Core**, it provides a scalable architecture for Event Organisers to publish events, manage categories, and record results, while allowing Participants to browse events and manage race enrolments securely.

---

## Tech Stack & Architecture
- **Framework:** ASP.NET Core 8 Web API
- **Database & ORM:** SQL Server / LocalDB with Entity Framework Core 8
- **Authentication & Security:** JWT (JSON Web Tokens) with Role-Based Access Control (RBAC)
- **Testing:** xUnit testing framework with EF Core InMemory database provider
- **CI/CD:** Automated GitHub Actions workflows for documentation and build validation

---

## Core System Roles & Permissions
- **Organiser:** Full administrative control to create/manage events, assign categories, oversee enrolments, and record verified participant results. Protected via `[Authorize(Roles = "Organiser")]`.
- **Participant:** Can register user accounts, browse available race events and categories, and manage personal race enrolments. Protected via JWT authentication.

---

## Local Database & Environment Setup

### 1. Prerequisites
- .NET 8 SDK
- Microsoft SQL Server 2019/2022 or LocalDB
- SQL Server Management Studio (SSMS) or Azure Data Studio

### 2. Configuration (`appsettings.json`)
Ensure your connection string is properly configured in `appsettings.json` (or use `appsettings.Example.json` as a template):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_BYTES_LONG",
    "Issuer": "RaceDayAPI",
    "Audience": "RaceDayUsers"
  }
}
```
3. Database Execution

    Launch SQL Server Management Studio and connect to your local server instance ((localdb)\mssqllocaldb).

    Open docs/RaceDay_Database.sql inside SSMS.

    Execute the script (F5) to generate the RaceDayDb database and schema.

Running Automated Tests

The solution includes a dedicated unit testing project (RaceDay_EventMS.Tests) utilizing xUnit and EF Core InMemory.

To execute the test suite:
Bash

dotnet test

Project Structure

    RaceDay_EventMS/ - Main ASP.NET Core Web API project (Controllers, Models, Data context).

    RaceDay_EventMS.Tests/ - xUnit automated unit testing suite.

    .github/workflows/ - CI/CD pipeline validation workflows.

    docs/ - System architecture documentation, ERD diagrams, and database initialization scripts.

 ## CI/CD Status

![CI Build Status](../../workflows/Validate%20Documentation%20Folder/badge.svg) 
Video Presentation

    Part 1 Walkthrough: Watch on YouTube

    Part 2 System & Backend Walkthrough:https://youtu.be/2sOEjsGWI7Y
