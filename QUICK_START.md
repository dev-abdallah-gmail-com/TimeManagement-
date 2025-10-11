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

### Step 4: Create Tags (Optional but Recommended)
```bash
curl -X POST http://localhost:5154/api/tags \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Bug",
    "color": "#e74c3c"
  }'
```

### Step 5: Create a Task
```bash
curl -X POST http://localhost:5154/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "title": "My First Task",
    "description": "Task description",
    "scheduledStartDate": "2025-10-15T09:00:00Z",
    "scheduledEndDate": "2025-10-20T17:00:00Z",
    "tagIds": [1],
    "assigneeEmail": "user@example.com"
  }'
```

### Step 6: View Your Tasks
```bash
# Get all tasks (calendar view)
curl -X GET http://localhost:5154/api/tasks/all \
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

## 🎯 Task Workflow (Updated)

### Standard Workflow:
```
1. Create Task (Status: Pending)
   - Can optionally assign during creation
   - Can add tags for categorization
   
2. Assign to User (Status: Assigned)
   - Creator can assign to anyone
   - User can self-assign
   - Can be unassigned (back to Pending)
   
3. User Accepts (Status: Accepted)
   - Alternative: User Rejects (Status: Rejected, unassigned)
   
4. User Works on It (Status: InProgress)
   - Requires scheduledStartDate and scheduledEndDate
   
5. User Completes with Actual Time (Status: PendingApproval)
   - Must provide actualStartDate and actualEndDate
   
6. Owner Approves (Status: Approved) - FINAL
   - Alternative: Owner Rejects (back to Assigned)
```

### New Features:
- **Tags**: Color-coded labels for tasks
- **History**: Full audit trail of all actions
- **Actual Time**: Track real time spent
- **Approval**: Owner reviews completed work
- **Calendar View**: Get all tasks with time frames
- **Business Rules**: InProgress requires time frame

## ⚡ Quick Tips

1. **Keep your token safe**: It expires in 7 days
2. **Check task ownership**: You can only see tasks you created or are assigned to
3. **Modify freely**: Until a task is approved/completed, you can modify it
4. **Approved tasks are final**: Cannot modify or delete
5. **Creator or user can assign**: Task creator or user themselves can assign tasks
6. **Only assignee can accept/reject**: Acceptance is assignee's choice
7. **Time frames required**: Must set scheduledStartDate/EndDate before InProgress
8. **Actual time required**: Must provide actual time when completing
9. **Owner approves**: Task creator reviews and approves/rejects completion
10. **Use tags**: Color-coded tags help organize tasks visually
11. **Check history**: View full audit trail of task changes
12. **Calendar view**: Use /api/tasks/all for unified view

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

## 🆕 New Features Quick Reference

### Admin User Management
```bash
# List all users (admin only)
curl -X GET http://localhost:5154/api/admin/users \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### Tags
```bash
# Create tag
curl -X POST http://localhost:5154/api/tags \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Urgent", "color": "#ff0000"}'

# List tags
curl -X GET http://localhost:5154/api/tags \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Task History
```bash
# View complete history
curl -X GET http://localhost:5154/api/tasks/1/history \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Complete Task with Actual Time
```bash
curl -X POST http://localhost:5154/api/tasks/1/complete \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "actualStartDate": "2025-10-15T09:15:00Z",
    "actualEndDate": "2025-10-15T16:30:00Z"
  }'
```

### Approve/Reject Task
```bash
# Approve
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"approve": true}'

# Reject
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"approve": false, "rejectionReason": "Needs more work"}'
```

### Unassign Task
```bash
curl -X POST http://localhost:5154/api/tasks/1/assign \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Calendar View
```bash
# Get all tasks (created or assigned to you)
curl -X GET http://localhost:5154/api/tasks/all \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📚 More Information

- **Detailed Documentation**: See `README.md`
- **New Features Guide**: See `ENHANCEMENTS.md`
- **Test Scenarios**: See `API_TEST_SCENARIOS.md`
- **Implementation Details**: See `IMPLEMENTATION_DETAILS.md`
- **Architecture Details**: See `ARCHITECTURE.md`

## 🎉 You're Ready!

Start building your task management workflow with TimeManagement API!

For questions or issues, refer to the comprehensive documentation files.
