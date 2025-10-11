# TimeManagement API - Architecture Documentation

## Overview

The TimeManagement API is a RESTful web service built using ASP.NET Core 9.0 that provides a comprehensive task management system with role-based authentication and a complete task lifecycle workflow.

## Architecture Pattern

The application follows a **layered architecture** pattern:

```
┌─────────────────────────────────────────┐
│         Controllers Layer               │
│  (API Endpoints & Request Handling)     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│          Services Layer                 │
│   (Business Logic & Validation)         │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Data Access Layer               │
│    (Entity Framework Core & DbContext)  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│          Database (SQLite)              │
└─────────────────────────────────────────┘
```

## Project Structure

```
TimeManagement.Api/
│
├── Controllers/                 # API Controllers
│   ├── AuthController.cs       # Authentication endpoints
│   └── TasksController.cs      # Task management endpoints
│
├── Services/                    # Business Logic Layer
│   ├── IAuthService.cs         # Authentication service interface
│   ├── AuthService.cs          # Authentication implementation
│   ├── ITaskService.cs         # Task service interface
│   └── TaskService.cs          # Task service implementation
│
├── Models/                      # Domain Models
│   ├── ApplicationUser.cs      # User entity (extends IdentityUser)
│   ├── UserTask.cs             # Task entity
│   └── TaskStatus.cs           # Task status enum
│
├── DTOs/                        # Data Transfer Objects
│   ├── RegisterDto.cs          # User registration request
│   ├── LoginDto.cs             # Login request
│   ├── AuthResponseDto.cs      # Authentication response
│   ├── CreateTaskDto.cs        # Create task request
│   ├── UpdateTaskDto.cs        # Update task request
│   ├── AssignTaskDto.cs        # Assign task request
│   ├── AcceptRejectTaskDto.cs  # Accept/Reject task request
│   └── TaskResponseDto.cs      # Task response
│
├── Data/                        # Data Access Layer
│   └── ApplicationDbContext.cs # EF Core DbContext
│
├── Program.cs                   # Application entry point & configuration
├── appsettings.json            # Configuration settings
└── TimeManagement.Api.csproj   # Project file
```

## Technology Stack

### Core Technologies
- **ASP.NET Core 9.0**: Web API framework
- **C# 13**: Programming language
- **Entity Framework Core 9.0**: ORM for data access
- **SQLite**: Lightweight database
- **ASP.NET Core Identity**: User management and authentication
- **JWT (JSON Web Tokens)**: Token-based authentication
- **Swashbuckle/Swagger**: API documentation

### Key NuGet Packages
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `Swashbuckle.AspNetCore`

## Data Model

### Entity Relationship Diagram

```
┌─────────────────────────┐
│    ApplicationUser      │
│─────────────────────────│
│ Id (PK)                 │
│ Email                   │
│ UserName                │
│ FirstName               │
│ LastName                │
│ PasswordHash            │
│ CreatedAt               │
│ ... (Identity fields)   │
└─────────────────────────┘
            │
            │ 1
            │
            │ *
┌─────────────────────────┐
│       UserTask          │
│─────────────────────────│
│ Id (PK)                 │
│ Title                   │
│ Description             │
│ ScheduledStartDate      │
│ ScheduledEndDate        │
│ Status                  │
│ CreatedBy (FK)          │─────┐
│ AssignedTo (FK)         │─────┘
│ CreatedAt               │
│ UpdatedAt               │
│ CompletedAt             │
│ RejectionReason         │
└─────────────────────────┘
```

### Task Status Flow

```
Pending (0) → Assigned (1) → Accepted (2) → InProgress (3) → Completed (4)
                     ↓
                Rejected (5)
```

## Authentication & Authorization

### JWT Token Authentication

1. **User Registration/Login**: User provides credentials
2. **Token Generation**: Server generates JWT token with user claims
3. **Token Storage**: Client stores token (localStorage/sessionStorage)
4. **Authenticated Requests**: Client includes token in Authorization header
5. **Token Validation**: Server validates token and extracts user identity

### JWT Token Structure

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "name": "username",
  "role": "User",
  "exp": 1760746823,
  "iss": "TimeManagementApi",
  "aud": "TimeManagementClient"
}
```

### Role-Based Authorization

- **User Role**: Standard users who can create and manage their own tasks
- **Admin Role**: Administrators with elevated privileges

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/register-admin` | Register admin | No |
| POST | `/api/auth/login` | User login | No |

### Task Management Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/tasks` | Create new task | Yes |
| GET | `/api/tasks/{id}` | Get task by ID | Yes |
| GET | `/api/tasks/my-tasks` | Get tasks created by user | Yes |
| GET | `/api/tasks/assigned-to-me` | Get tasks assigned to user | Yes |
| PUT | `/api/tasks/{id}` | Update task details | Yes |
| DELETE | `/api/tasks/{id}` | Delete task | Yes |
| POST | `/api/tasks/{id}/assign` | Assign task to user | Yes |
| POST | `/api/tasks/{id}/accept-reject` | Accept/Reject assignment | Yes |
| PATCH | `/api/tasks/{id}/status` | Update task status | Yes |

## Business Rules

### Task Creation
- Any authenticated user can create a task
- New tasks start with status "Pending"
- Creator is automatically set to the current user

### Task Assignment
- Only the task creator can assign tasks
- Tasks can be assigned to any registered user
- Assignment changes status to "Assigned"
- Assignee receives the task in their "assigned-to-me" list

### Task Acceptance/Rejection
- Only the assignee can accept or reject a task
- Tasks must be in "Assigned" status
- **Accept**: Changes status to "Accepted"
- **Reject**: Changes status to "Rejected", unassigns the task, stores reason

### Task Modification
- Creator or assignee can modify task details
- Tasks cannot be modified once completed
- Status can be updated by creator or assignee
- Deletion is only allowed by the creator
- Completed tasks cannot be deleted

### Task Completion
- Marking as completed sets the CompletedAt timestamp
- Completed tasks become read-only

### Access Control
- Users can only view tasks they created or are assigned to
- All operations validate user permissions

## Security Features

### Authentication Security
- Passwords hashed using ASP.NET Core Identity
- JWT tokens with expiration (7 days)
- Token signature validation
- HTTPS enforcement in production

### Authorization Security
- Role-based access control
- Claims-based authorization
- User-level data isolation
- Input validation on all endpoints

### Data Security
- SQL injection protection via EF Core
- Parameter validation via Data Annotations
- CORS configuration (configurable per environment)

## Configuration

### Application Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=timemanagement.db"
  },
  "Jwt": {
    "Key": "SECRET_KEY_HERE",
    "Issuer": "TimeManagementApi",
    "Audience": "TimeManagementClient"
  }
}
```

### Environment Variables (Recommended for Production)

- `JWT_KEY`: Secret key for JWT signing
- `JWT_ISSUER`: JWT issuer
- `JWT_AUDIENCE`: JWT audience
- `CONNECTION_STRING`: Database connection string

## Database Schema

### AspNetUsers Table
- Stores user information
- Managed by ASP.NET Core Identity

### AspNetRoles Table
- Stores role definitions (Admin, User)
- Managed by ASP.NET Core Identity

### Tasks Table
```sql
CREATE TABLE Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    ScheduledStartDate TEXT,
    ScheduledEndDate TEXT,
    Status INTEGER NOT NULL,
    CreatedBy TEXT NOT NULL,
    AssignedTo TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    CompletedAt TEXT,
    RejectionReason TEXT,
    FOREIGN KEY (CreatedBy) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (AssignedTo) REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);
```

## Deployment Considerations

### Development
- SQLite for simplicity
- Swagger UI enabled
- Detailed logging

### Production
- Consider SQL Server/PostgreSQL for scalability
- Disable Swagger
- Configure CORS appropriately
- Use environment variables for secrets
- Enable HTTPS
- Implement rate limiting
- Add monitoring and logging

## Future Enhancements

### Potential Features
1. Task comments and activity log
2. File attachments
3. Task priorities and categories
4. Email notifications
5. Task dependencies
6. Recurring tasks
7. Team/project grouping
8. Advanced filtering and search
9. Task templates
10. Time tracking
11. Dashboard and analytics
12. Mobile application
13. Real-time updates with SignalR
14. Task delegation
15. Approval workflows

### Technical Improvements
1. Unit and integration tests
2. API versioning
3. Response caching
4. Rate limiting
5. Health checks
6. Distributed caching (Redis)
7. Message queue integration
8. Microservices architecture
9. GraphQL API
10. Containerization (Docker)

## Error Handling

The API uses standard HTTP status codes:

- **200 OK**: Successful request
- **201 Created**: Resource created successfully
- **204 No Content**: Successful deletion
- **400 Bad Request**: Invalid input
- **401 Unauthorized**: Authentication required or failed
- **403 Forbidden**: User lacks permission
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

Error responses include descriptive messages:

```json
{
  "message": "Task not found or you don't have permission to access it."
}
```

## Conclusion

The TimeManagement API provides a robust, secure, and scalable foundation for task management with a clean architecture that separates concerns and follows best practices for ASP.NET Core development.
