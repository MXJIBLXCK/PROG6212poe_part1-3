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
3. Run the API & Tests

    To run the API: Open your terminal in the solution folder and run:
    Bash

    dotnet run --project RaceDay_EventMS

    To run the unit tests: Run the xUnit test suite with:
    Bash

    dotnet test

### Running Locally

    This will start:
    - **API**: http://localhost:7105
    - **Web Application**: http://localhost:5109

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

---

## Architecture

### API (RaceDay_EventMS)
- RESTful API endpoints
- JWT authentication
- Role-based authorization (Organiser/Participant)
- Entity Framework Core ORM
- Azure Blob Storage integration

### Web Application (RaceDay_EventMS.Web)
- MVC architecture with Razor Views
- Session-based authentication
- Bootstrap 5 UI framework
- Responsive design
- Client-side validation

## UI Design

The application follows RaceDay brand guidelines with:
- **Sporting Theme**: Energetic, modern design reflecting athletic events
- **Colour Scheme**: Professional blues, greens, and accents
- **Accessibility**: WCAG 2.1 compliant
- **Responsiveness**: Mobile-first Bootstrap 5 framework
- **Status Indicators**: Colour-coded enrolment and result statuses

## Environment Configuration

### API (appsettings.json)

### Web App (appsettings.json)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Events
- `GET /api/events` - Get all events
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create event (Organiser only)
- `PUT /api/events/{id}` - Update event (Organiser only)
- `DELETE /api/events/{id}` - Delete event (Organiser only)

### Categories
- `GET /api/categories` - Get all categories
- `POST /api/categories` - Add category to event

### Enrolments
- `GET /api/enrolments` - Get all enrolments
- `POST /api/categories/{categoryId}/enrolments` - Enrol in category
- `DELETE /api/enrolments/{id}` - Cancel enrolment

### Results
- `GET /api/categories/{categoryId}/results` - Get leaderboard
- `POST /api/categories/{categoryId}/results` - Submit result

## Docker Configuration

### API Dockerfile
Multi-stage build for optimized image size:
1. Build stage: Compiles application
2. Runtime stage: Runs compiled application

### Web App Dockerfile
Similar multi-stage build structure

### Docker Compose
Orchestrates both API and Web services with proper networking and port mapping.

**Run with single command**:

---

## UI/UX Guidelines

### Consistent Design
- Unified colour palette across all pages
- Bootstrap 5 framework for responsive layouts
- Consistent typography and spacing

### Error Handling
- User-friendly error messages
- Graceful degradation
- Toast notifications for feedback

### Navigation
- Role-based menu items
- Breadcrumb navigation
- Clear action buttons

### Accessibility
- Alt text for images
- ARIA labels for screen readers
- Keyboard navigation support
- High contrast text

## Contributing
Please follow the existing code structure and ensure all new features include proper error handling and validation.

## License
Internal Use Only
