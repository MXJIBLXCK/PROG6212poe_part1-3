cat << 'EOF' > docs/API_Endpoint_Plan.md
# RaceDay - API Endpoint Specification Plan

**System Overview:** RaceDay Event Management System RESTful API Architecture  
**Document Version:** 1.0.0  
**Target Architecture:** ASP.NET Core Web API (.NET 8/9) with JWT Authentication  

---

## 1. Architectural Strategy & Conventions

The RaceDay API follows strict RESTful design principles, structured JSON payload standards, and HTTP response code conventions.

### Base URL Structure
- **Development:** `https://localhost:7123/api/v1`
- **Production:** `https://api.raceday.co.za/api/v1`

### Global Standard Headers
| Header Name | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `Content-Type` | String | Yes | `application/json` |
| `Accept` | String | Yes | `application/json` |
| `Authorization` | String | Conditional | `Bearer <JWT_TOKEN>` (Required for protected endpoints) |

---

## 2. Global HTTP Response Codes & Error Envelope

All API responses return a standardized wrapper payload.

### Standard Success Response (HTTP 200 / 201)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation completed successfully.",
  "data": {}
}