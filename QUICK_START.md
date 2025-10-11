# TimeManagement API - Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Prerequisites
- .NET 9.0 SDK installed

### Step 1: Clone and Build
```bash
git clone <repository-url>
cd TimeManagement-/TimeManagement.Api
dotnet restore
dotnet build
```

### Step 2: Run the Application
```bash
dotnet run
```

The API will start at: `http://localhost:5154`
Swagger UI: `http://localhost:5154/swagger`

### Step 3: Register a User
```bash
curl -X POST http://localhost:5154/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response:** You'll receive a JWT token. Save it!

### Step 4: Create a Task
```bash
curl -X POST http://localhost:5154/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "title": "My First Task",
    "description": "Task description",
    "scheduledStartDate": "2025-10-15T09:00:00Z",
    "scheduledEndDate": "2025-10-20T17:00:00Z"
  }'
```

### Step 5: View Your Tasks
```bash
curl -X GET http://localhost:5154/api/tasks/my-tasks \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 📖 Common Operations

### Register Another User
```bash
curl -X POST http://localhost:5154/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "jane@example.com",
    "password": "Password123!",
    "firstName": "Jane",
    "lastName": "Smith"
  }'
```

### Assign Task to Someone
```bash
curl -X POST http://localhost:5154/api/tasks/1/assign \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "assigneeEmail": "jane@example.com"
  }'
```

### Accept a Task (as the assignee)
```bash
curl -X POST http://localhost:5154/api/tasks/1/accept-reject \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer JANE_TOKEN" \
  -d '{
    "accept": true
  }'
```

### Update Task Status to InProgress
```bash
curl -X PATCH http://localhost:5154/api/tasks/1/status \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '3'
```

Status values:
- 0 = Pending
- 1 = Assigned
- 2 = Accepted
- 3 = InProgress
- 4 = Completed
- 5 = Rejected

### Complete a Task
```bash
curl -X PATCH http://localhost:5154/api/tasks/1/status \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '4'
```

## 🧪 Using the Test Script

Run the automated test script:
```bash
./test-api.sh
```

This will:
1. Register two users (John & Jane)
2. John creates a task
3. John assigns it to Jane
4. Jane accepts the task
5. Jane updates it to InProgress
6. Jane modifies task details
7. Jane completes the task
8. Display all tasks

## 🌐 Using Swagger UI

1. Start the application: `dotnet run`
2. Open browser: `http://localhost:5154/swagger`
3. Click "Authorize" button
4. Register a user via `/api/auth/register`
5. Login via `/api/auth/login` to get token
6. Enter token in format: `Bearer YOUR_TOKEN_HERE`
7. Test all endpoints interactively

## 📱 Using Postman

1. Import `API_COLLECTION.json` into Postman
2. Set the `baseUrl` variable to `http://localhost:5154`
3. Register/Login to get a token
4. Set the `token` variable with your JWT token
5. All requests will automatically use the token

## 🔑 Password Requirements

Passwords must have:
- At least 6 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit

## 🎯 Task Workflow

```
1. Create Task (Status: Pending)
2. Assign to User (Status: Assigned)
3. User Accepts (Status: Accepted)
4. User Works on It (Status: InProgress)
5. User Completes (Status: Completed)
```

Alternative: User can reject at step 3 (Status: Rejected)

## ⚡ Quick Tips

1. **Keep your token safe**: It expires in 7 days
2. **Check task ownership**: You can only see tasks you created or are assigned to
3. **Modify freely**: Until a task is completed, you can modify it
4. **Completed tasks are final**: Cannot modify or delete
5. **Only creator can assign**: Task assignment is creator's privilege
6. **Only assignee can accept/reject**: Acceptance is assignee's choice

## 🐛 Troubleshooting

### Application won't start
```bash
dotnet clean
dotnet restore
dotnet build
dotnet run
```

### Can't access API
- Check if running: Look for "Now listening on: http://localhost:5154"
- Try different port in launchSettings.json
- Disable firewall temporarily

### Authentication fails
- Check token format: `Bearer YOUR_TOKEN`
- Token might be expired (7 days validity)
- Login again to get new token

### Can't see tasks
- Verify you created or are assigned to the task
- Check Authorization header is present
- Ensure token is valid

## 📚 More Information

- **Detailed Documentation**: See `README.md`
- **Architecture Details**: See `ARCHITECTURE.md`
- **Implementation Summary**: See `IMPLEMENTATION_SUMMARY.md`

## 🎉 You're Ready!

Start building your task management workflow with TimeManagement API!

For questions or issues, refer to the comprehensive documentation files.
