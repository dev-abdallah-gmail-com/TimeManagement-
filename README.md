# TimeManagement API

A role-based task management system built with ASP.NET Core 9.0 that enables users to create, assign, schedule, and manage tasks with acceptance workflows.

## Features

- **Role-Based Authentication**: Support for Admin and User roles with JWT authentication
- **Admin User Management**: Admins can view and manage all users
- **Task Management**: Create, read, update, and delete tasks
- **Task Assignment**: Users can assign tasks to themselves or others, with support for unassignment
- **Task Tags**: Organize tasks with colored tags for visual categorization
- **Task History**: Full audit trail of all task changes and actions
- **Acceptance Workflow**: Assignees must accept or reject tasks before working on them
- **Task Scheduling**: Schedule tasks with start and end dates (time frames)
- **Actual Time Tracking**: Track actual time spent on tasks before completion
- **Approval Workflow**: Task owners review and approve/reject completed tasks
- **Business Rules**: Tasks require time frames before setting to InProgress status
- **Status Management**: Track task status (Pending, Assigned, Accepted, InProgress, PendingApproval, Approved, Rejected)
- **Calendar View Support**: Get all tasks for unified calendar-style views
- **Interfering Tasks**: Tasks can have overlapping time frames
- **API Documentation**: Integrated Swagger/OpenAPI documentation

## Technology Stack

- ASP.NET Core 9.0 Web API
- Entity Framework Core 9.0
- SQLite Database
- ASP.NET Core Identity for authentication
- JWT Bearer Authentication
- Swagger/OpenAPI for API documentation

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TimeManagement-
```

### 2. Build the Project

```bash
cd TimeManagement.Api
dotnet build
```

### 3. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

### 4. Database

The application uses SQLite and will automatically create the database (`timemanagement.db`) on first run with the required schema and roles (Admin, User).

## API Endpoints

### Authentication

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Register Admin
```http
POST /api/auth/register-admin
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Password123!",
  "firstName": "Admin",
  "lastName": "User"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

Response includes JWT token to be used in subsequent requests.

### Task Management

All task endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

#### Create Task
```http
POST /api/tasks
Content-Type: application/json

{
  "title": "Complete Project Documentation",
  "description": "Write comprehensive documentation for the project",
  "scheduledStartDate": "2025-10-15T09:00:00Z",
  "scheduledEndDate": "2025-10-20T17:00:00Z"
}
```

#### Get Task by ID
```http
GET /api/tasks/{id}
```

#### Get My Tasks (Created by Me)
```http
GET /api/tasks/my-tasks
```

#### Get Tasks Assigned to Me
```http
GET /api/tasks/assigned-to-me
```

#### Update Task
```http
PUT /api/tasks/{id}
Content-Type: application/json

{
  "title": "Updated Task Title",
  "description": "Updated description",
  "scheduledStartDate": "2025-10-15T09:00:00Z",
  "scheduledEndDate": "2025-10-20T17:00:00Z"
}
```

#### Delete Task
```http
DELETE /api/tasks/{id}
```

#### Assign Task to User
```http
POST /api/tasks/{id}/assign
Content-Type: application/json

{
  "assigneeEmail": "assignee@example.com"
}
```

#### Accept/Reject Task
```http
POST /api/tasks/{id}/accept-reject
Content-Type: application/json

{
  "accept": true,
  "rejectionReason": null
}
```

Or to reject:
```json
{
  "accept": false,
  "rejectionReason": "Not enough time to complete this task"
}
```

#### Update Task Status
```http
PATCH /api/tasks/{id}/status
Content-Type: application/json

3
```

Status values:
- 0: Pending
- 1: Assigned
- 2: Accepted
- 3: InProgress
- 4: Completed
- 5: Rejected

## Task Workflow

1. **Create Task**: User creates a task (Status: Pending)
2. **Assign Task**: Task creator assigns it to another user (Status: Assigned)
3. **Accept/Reject**: Assignee must accept or reject the task
   - Accept: Task status becomes Accepted
   - Reject: Task status becomes Rejected and is unassigned
4. **Work on Task**: Assignee can update status to InProgress
5. **Complete Task**: User marks task as Completed
6. **Modify Anytime**: Creator or assignee can modify task details if not completed

## Business Rules

- Only the task creator can assign tasks to others
- Only the assignee can accept or reject a task
- Tasks can only be modified if they're not completed
- Only the creator can delete tasks
- Completed tasks cannot be modified or deleted
- Users can only view tasks they created or are assigned to

## Security

- Passwords must be at least 6 characters with uppercase, lowercase, and digits
- JWT tokens expire after 7 days
- Role-based authorization ensures proper access control
- CORS is enabled for development (should be restricted in production)

## Configuration

Key configuration settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=timemanagement.db"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForSecurity!",
    "Issuer": "TimeManagementApi",
    "Audience": "TimeManagementClient"
  }
}
```

**Important**: Change the JWT Key in production to a secure random value!

## Development

### Project Structure

```
TimeManagement.Api/
├── Controllers/          # API controllers
│   ├── AuthController.cs
│   └── TasksController.cs
├── Data/                # Database context
│   └── ApplicationDbContext.cs
├── DTOs/                # Data transfer objects
├── Models/              # Domain models
│   ├── ApplicationUser.cs
│   ├── TaskStatus.cs
│   └── UserTask.cs
└── Services/            # Business logic
    ├── AuthService.cs
    ├── IAuthService.cs
    ├── TaskService.cs
    └── ITaskService.cs
```

### Testing with Swagger

1. Run the application
2. Navigate to `https://localhost:5001/swagger`
3. Register a user using `/api/auth/register`
4. Login to get a JWT token
5. Click "Authorize" button and enter: `Bearer <your-token>`
6. Test the API endpoints

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.