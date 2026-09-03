# RaceDay - API Endpoint Specification Plan

**System Overview:** RaceDay Event Management System RESTful API Architecture  
**Document Version:** 1.0.0  
**Target Architecture:** ASP.NET Core Web API (.NET 8/9) with JWT Authentication  

---

## 1. Overview & Architectural Conventions

This document outlines all RESTful API endpoints for the RaceDay Event Management System. All endpoints reside under the base route `/api/v1/`.

* **Global Content-Type:** `application/json`
* **Authentication Standard:** JSON Web Token (JWT) sent via `Authorization: Bearer <TOKEN>` header.
* **Standard Response Envelope:**
  * Success: `{ "success": true, "statusCode": int, "message": "string", "data": {} }`
  * Error: `{ "success": false, "statusCode": int, "message": "string", "errors": [] }`

---

## 2. Master API Endpoint Table

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/v1/auth/register` | Registers a new account as an Organiser or Participant. | None (Public) | `{ email, password, fullName, role }` | **201 Created** - User object<br>**400 Bad Request** - Validation failure<br>**409 Conflict** - Email registered |
| **POST** | `/api/v1/auth/login` | Authenticates credentials and returns a JWT access token. | None (Public) | `{ email, password }` | **200 OK** - JWT bearer token and role<br>**401 Unauthorized** - Invalid credentials |
| **GET** | `/api/v1/users/profile` | Retrieves extended profile information for the authenticated user. | Any (Logged in) | None | **200 OK** - Profile details<br>**401 Unauthorized** - Invalid token<br>**404 Not Found** - Profile missing |
| **PUT** | `/api/v1/users/profile` | Creates or updates extended profile details for the logged-in user. | Any (Logged in) | `{ phoneNumber, emergencyContactName, emergencyContactPhone, runningClub }` | **200 OK** - Updated profile object<br>**400 Bad Request** - Validation failure<br>**401 Unauthorized** - Token missing |
| **GET** | `/api/v1/events` | Retrieves a list of all active race events. | None (Public) | None | **200 OK** - Array of event items |
| **GET** | `/api/v1/events/{id}` | Retrieves detailed information for a specific event by ID. | None (Public) | None | **200 OK** - Event object<br>**404 Not Found** - Event ID missing |
| **POST** | `/api/v1/events` | Creates a new race event in the system. | Organiser | `{ name, description, date, location }` | **201 Created** - Event object<br>**400 Bad Request** - Invalid fields<br>**403 Forbidden** - Not an Organiser |
| **PUT** | `/api/v1/events/{id}` | Updates details for an existing race event. | Organiser | `{ name, description, date, location }` | **200 OK** - Updated event object<br>**403 Forbidden** - Unauthorized<br>**404 Not Found** - Event missing |
| **DELETE** | `/api/v1/events/{id}` | Deletes a race event from the database. | Organiser | None | **204 No Content** - Successfully deleted<br>**403 Forbidden** - Unauthorized<br>**404 Not Found** - Event missing |
| **GET** | `/api/v1/events/{eventId}/categories` | Lists all race categories associated with a specific event. | None (Public) | None | **200 OK** - Array of category items<br>**404 Not Found** - Event missing |
| **POST** | `/api/v1/events/{eventId}/categories` | Adds a new category (distance, fee, cut-off) to an event. | Organiser | `{ name, distanceKm, entryFee, maxParticipants }` | **201 Created** - Category object<br>**400 Bad Request** - Invalid input<br>**403 Forbidden** - Not Organiser |
| **POST** | `/api/v1/categories/{categoryId}/enrolments` | Enrols the authenticated participant into a specific event category. | Participant | None | **201 Created** - Enrolment record<br>**401 Unauthorized** - Token missing<br>**409 Conflict** - Duplicate entry |
| **GET** | `/api/v1/users/enrolments` | Retrieves all event enrolments for the currently logged-in participant. | Participant | None | **200 OK** - List of user enrolments<br>**401 Unauthorized** - Token missing |
| **DELETE** | `/api/v1/enrolments/{id}` | Cancels an existing event enrolment. | Participant | None | **204 No Content** - Cancelled<br>**403 Forbidden** - Not owner<br>**404 Not Found** - Entry missing |
| **POST** | `/api/v1/categories/{categoryId}/results` | Records timing and finish position results for a participant. | Organiser | `{ participantId, finishTimeSeconds, position }` | **201 Created** - Result entry<br>**400 Bad Request** - Invalid timing data<br>**403 Forbidden** - Not Organiser |
| **GET** | `/api/v1/categories/{categoryId}/results` | Retrieves official race leaderboard results for a specific category. | None (Public) | None | **200 OK** - Sorted leaderboard array<br>**404 Not Found** - Category missing |