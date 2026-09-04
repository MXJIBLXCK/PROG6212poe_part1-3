# RaceDay Database Data Dictionary

| Table | Column | Data Type | Nullable | Primary Key | Foreign Key | Description |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Users** | `UserId` | `INT` | No | Yes | No | Auto-incrementing user identity. |
| **Users** | `Email` | `NVARCHAR(150)` | No | No | No | Unique account login email. |
| **Users** | `Role` | `NVARCHAR(20)` | No | No | No | User system role (`Organiser` or `Participant`). |
| **Events** | `EventId` | `INT` | No | Yes | No | Auto-incrementing event identity. |
| **Events** | `OrganiserId` | `INT` | No | No | Yes (`Users`) | Links event to creating organiser. |
| **Categories** | `CategoryId` | `INT` | No | Yes | No | Auto-incrementing category identity. |
| **Categories** | `DistanceKm` | `DECIMAL(5,2)`| No | No | No | Race distance in kilometers. |
| **Enrolments**| `EnrolmentId` | `INT` | No | Yes | No | Auto-incrementing registration identity. |
| **Results** | `ResultId` | `INT` | No | Yes | No | Auto-incrementing result record identity. |