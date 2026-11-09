# RaceDay - Event Management System

**Course Code:** PROG6212  
**Portfolio of Evidence (POE)**

**Youtube link:** https://youtu.be/eG07HlSwstQ

## Overview
RaceDay EventMS is a robust, multi-tier web application designed for organizing, managing, and tracking endurance racing events (Running, Cycling, Trail, and Triathlons). The system is split into a secure backend RESTful Web API and a responsive ASP.NET Core MVC frontend interface, backed by a Microsoft SQL Server database.

---

## Tech Stack & Architecture
System Architecture & Tech Stack

    Backend: ASP.NET Core Web API (Port 5059 / Port 5109 configuration)

    Frontend: ASP.NET Core MVC Web Application

    Database: Microsoft SQL Server Express (LabVM204993\SQLEXPRESS, RaceDayDB)

    Authentication: JWT (JSON Web Tokens) secured session storage (JWToken, UserId, UserRole)

    Key Features: Organiser event CRUD operations, category mapping, participant enrolment tracking, and official race results leaderboard entry.
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

### 2. How to Run the Project

To run the system locally, open two separate Developer PowerShell terminals and follow these steps:
Terminal 1: Start the Backend API

    Navigate to the API project directory:
    PowerShell

    cd RaceDay_EventMS

    Start the API server:

    dotnet run

Terminal 2: Start the Web MVC Frontend

    Navigate to the Web project directory:
    PowerShell

    cd RaceDay_EventMS.Web

    Start the web application:

    dotnet run

    Copy the running localhost URL (http://localhost:5109 or **http://localhost:5090** as indicated in your terminal output), paste it into your web browser, and log in with your organiser account to begin managing events.

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
