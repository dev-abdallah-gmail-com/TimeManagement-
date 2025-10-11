# TimeManagement API - Test Scenarios

This document provides comprehensive test scenarios for all API features, including the new enhancements.

## Setup

```bash
# Start the API
cd TimeManagement.Api
dotnet run
```

The API will be available at `http://localhost:5154`

## Test Scenario 1: User Registration and Admin Access

```bash
# Register an admin user
curl -X POST http://localhost:5154/api/auth/register-admin \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Admin123",
    "firstName": "Admin",
    "lastName": "User"
  }'

# Save the token from the response
ADMIN_TOKEN="<token-from-response>"

# Register a regular user
curl -X POST http://localhost:5154/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@test.com",
    "password": "User123",
    "firstName": "John",
    "lastName": "Doe"
  }'

USER_TOKEN="<token-from-response>"

# Register another user
curl -X POST http://localhost:5154/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "jane@test.com",
    "password": "User123",
    "firstName": "Jane",
    "lastName": "Smith"
  }'

USER2_TOKEN="<token-from-response>"
```

## Test Scenario 2: Admin User Management

```bash
# List all users (admin only)
curl -X GET http://localhost:5154/api/admin/users \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Get specific user
curl -X GET http://localhost:5154/api/admin/users/<user-id> \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Try as regular user (should fail)
curl -X GET http://localhost:5154/api/admin/users \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Test Scenario 3: Tag Management

```bash
# Create tags with different colors
curl -X POST http://localhost:5154/api/tags \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bug",
    "color": "#e74c3c"
  }'

curl -X POST http://localhost:5154/api/tags \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Feature",
    "color": "#3498db"
  }'

curl -X POST http://localhost:5154/api/tags \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Urgent",
    "color": "#f39c12"
  }'

# List all tags
curl -X GET http://localhost:5154/api/tags \
  -H "Authorization: Bearer $USER_TOKEN"

# Update a tag
curl -X PUT http://localhost:5154/api/tags/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Critical Bug",
    "color": "#c0392b"
  }'

# Get specific tag
curl -X GET http://localhost:5154/api/tags/1 \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Test Scenario 4: Task Creation with Tags and Assignment

```bash
# Create task with tags and assign to self
curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fix login issue",
    "description": "Users cannot login with special characters",
    "scheduledStartDate": "2025-10-12T09:00:00Z",
    "scheduledEndDate": "2025-10-12T17:00:00Z",
    "tagIds": [1],
    "assigneeEmail": "john@test.com"
  }'

# Create task with multiple tags and assign to another user
curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Implement dashboard",
    "description": "Create user dashboard with statistics",
    "scheduledStartDate": "2025-10-13T09:00:00Z",
    "scheduledEndDate": "2025-10-15T17:00:00Z",
    "tagIds": [2],
    "assigneeEmail": "jane@test.com"
  }'

# Create task without time frame
curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Research new technologies",
    "description": "Investigate React vs Vue",
    "tagIds": [2]
  }'
```

## Test Scenario 5: Task Assignment Operations

```bash
# Assign task to someone else
curl -X POST http://localhost:5154/api/tasks/3/assign \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "assigneeEmail": "jane@test.com"
  }'

# Unassign a task
curl -X POST http://localhost:5154/api/tasks/3/assign \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'

# Self-assign a task
curl -X POST http://localhost:5154/api/tasks/3/assign \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "assigneeEmail": "jane@test.com"
  }'
```

## Test Scenario 6: Business Rule - InProgress Requires Time Frame

```bash
# Try to set task without time frame to InProgress (should fail)
curl -X PATCH http://localhost:5154/api/tasks/3/status \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '3'

# Add time frame to task
curl -X PUT http://localhost:5154/api/tasks/3 \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Research new technologies",
    "description": "Investigate React vs Vue",
    "scheduledStartDate": "2025-10-14T09:00:00Z",
    "scheduledEndDate": "2025-10-14T17:00:00Z",
    "tagIds": [2]
  }'

# Now set to InProgress (should work)
curl -X PATCH http://localhost:5154/api/tasks/3/status \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '3'
```

## Test Scenario 7: Complete Task Workflow with Approval

```bash
# Step 1: Accept the task (as assignee)
curl -X POST http://localhost:5154/api/tasks/1/accept-reject \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "accept": true
  }'

# Step 2: Set to InProgress
curl -X PATCH http://localhost:5154/api/tasks/1/status \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '3'

# Step 3: Complete with actual time
curl -X POST http://localhost:5154/api/tasks/1/complete \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "actualStartDate": "2025-10-12T09:15:00Z",
    "actualEndDate": "2025-10-12T16:30:00Z"
  }'

# Step 4: Owner approves the completion
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approve": true
  }'
```

## Test Scenario 8: Rejection Workflow

```bash
# Complete task 2
curl -X POST http://localhost:5154/api/tasks/2/accept-reject \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "accept": true
  }'

curl -X PATCH http://localhost:5154/api/tasks/2/status \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '3'

curl -X POST http://localhost:5154/api/tasks/2/complete \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "actualStartDate": "2025-10-13T10:00:00Z",
    "actualEndDate": "2025-10-15T16:00:00Z"
  }'

# Owner rejects with reason
curl -X POST http://localhost:5154/api/tasks/2/approve-reject \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approve": false,
    "rejectionReason": "Please add more unit tests and improve documentation"
  }'

# Check task status (should be back to Assigned)
curl -X GET http://localhost:5154/api/tasks/2 \
  -H "Authorization: Bearer $USER2_TOKEN"
```

## Test Scenario 9: Task History Tracking

```bash
# View complete history of a task
curl -X GET http://localhost:5154/api/tasks/1/history \
  -H "Authorization: Bearer $USER_TOKEN"

# View history shows:
# - Task creation
# - Assignment actions
# - Status changes
# - Approval/rejection
# - Updates
```

## Test Scenario 10: Calendar View

```bash
# Get all tasks for calendar display
curl -X GET http://localhost:5154/api/tasks/all \
  -H "Authorization: Bearer $USER_TOKEN"

# Returns all tasks created by or assigned to the user
# Includes:
# - scheduledStartDate/scheduledEndDate for planned time
# - actualStartDate/actualEndDate for actual time
# - tags with colors for visual categorization
# - all task details
```

## Test Scenario 11: Interfering Tasks (Overlapping Time Frames)

```bash
# Create multiple tasks with overlapping time frames
curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Task A",
    "description": "First task",
    "scheduledStartDate": "2025-10-16T09:00:00Z",
    "scheduledEndDate": "2025-10-16T15:00:00Z",
    "tagIds": [1],
    "assigneeEmail": "john@test.com"
  }'

curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Task B",
    "description": "Overlapping task",
    "scheduledStartDate": "2025-10-16T12:00:00Z",
    "scheduledEndDate": "2025-10-16T18:00:00Z",
    "tagIds": [2],
    "assigneeEmail": "john@test.com"
  }'

# Both tasks are allowed (interfering tasks permitted)
curl -X GET http://localhost:5154/api/tasks/all \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Test Scenario 12: Update Task with Tags

```bash
# Update task and change tags
curl -X PUT http://localhost:5154/api/tasks/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fix critical login issue",
    "description": "Updated description with more details",
    "scheduledStartDate": "2025-10-12T08:00:00Z",
    "scheduledEndDate": "2025-10-12T18:00:00Z",
    "tagIds": [1, 3]
  }'

# View history to see the update
curl -X GET http://localhost:5154/api/tasks/1/history \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Test Scenario 13: Task Queries

```bash
# Get my created tasks
curl -X GET http://localhost:5154/api/tasks/my-tasks \
  -H "Authorization: Bearer $USER_TOKEN"

# Get tasks assigned to me
curl -X GET http://localhost:5154/api/tasks/assigned-to-me \
  -H "Authorization: Bearer $USER_TOKEN"

# Get specific task
curl -X GET http://localhost:5154/api/tasks/1 \
  -H "Authorization: Bearer $USER_TOKEN"

# Get all tasks (created or assigned)
curl -X GET http://localhost:5154/api/tasks/all \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Test Scenario 14: Error Cases

```bash
# Try to complete task without being assignee (should fail)
curl -X POST http://localhost:5154/api/tasks/2/complete \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "actualStartDate": "2025-10-13T10:00:00Z",
    "actualEndDate": "2025-10-13T16:00:00Z"
  }'

# Try to approve task without being owner (should fail)
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer $USER2_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approve": true
  }'

# Try to modify approved task (should fail)
curl -X PUT http://localhost:5154/api/tasks/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Try to modify",
    "description": "Should not work",
    "tagIds": []
  }'
```

## Expected Status Codes

- `200 OK` - Successful GET, PUT, PATCH, POST operations
- `201 Created` - Resource created successfully
- `204 No Content` - Successful DELETE operation
- `400 Bad Request` - Invalid data or business rule violation
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Insufficient permissions (e.g., non-admin accessing admin endpoints)
- `404 Not Found` - Resource doesn't exist or user doesn't have access

## Summary

All test scenarios validate:
- ✅ User registration and authentication
- ✅ Admin user management
- ✅ Tag creation and management
- ✅ Task creation with tags and assignment
- ✅ Self-assignment and unassignment
- ✅ Business rule: InProgress requires time frame
- ✅ Complete task workflow with actual time
- ✅ Approval/rejection by task owner
- ✅ Full task history tracking
- ✅ Calendar view with all tasks
- ✅ Interfering tasks (overlapping time frames)
- ✅ Proper error handling and permissions
