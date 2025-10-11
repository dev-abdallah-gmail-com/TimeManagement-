# TimeManagement API - Enhancements Documentation

This document describes the new features and enhancements added to the TimeManagement API.

## New Features

### 1. Task Tags with Colors

Tasks can now be tagged with custom labels that have colors. This helps organize and categorize tasks visually.

**Endpoints:**
- `GET /api/tags` - Get all tags
- `GET /api/tags/{id}` - Get a specific tag
- `POST /api/tags` - Create a new tag
- `PUT /api/tags/{id}` - Update a tag
- `DELETE /api/tags/{id}` - Delete a tag

**Example:**
```bash
# Create a tag
curl -X POST http://localhost:5154/api/tags \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bug",
    "color": "#e74c3c"
  }'
```

### 2. Task History Tracking

All task actions are now tracked in a history log. This includes:
- Task creation
- Status changes
- Assignments
- Updates
- Approvals/Rejections

**Endpoint:**
- `GET /api/tasks/{id}/history` - Get full history of a task

**Example Response:**
```json
[
  {
    "id": 1,
    "taskId": 1,
    "action": "Created",
    "performedBy": "user-id",
    "performedByEmail": "user@example.com",
    "details": "Task created: Fix login bug",
    "oldStatus": null,
    "newStatus": "Assigned",
    "performedAt": "2025-10-11T07:21:50Z"
  }
]
```

### 3. Enhanced Task Assignment

**Assign to Self:**
Users can now assign tasks to themselves.

**Unassignment:**
Tasks can be unassigned by passing `null` or empty assigneeEmail.

**Create with Assignment:**
Tasks can be assigned during creation.

**Example:**
```bash
# Create task and assign to someone
curl -X POST http://localhost:5154/api/tasks \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fix bug",
    "description": "Bug description",
    "scheduledStartDate": "2025-10-12T09:00:00Z",
    "scheduledEndDate": "2025-10-12T17:00:00Z",
    "tagIds": [1],
    "assigneeEmail": "user@example.com"
  }'

# Unassign task
curl -X POST http://localhost:5154/api/tasks/1/assign \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### 4. Business Rule: InProgress Requires Time Frame

Tasks can only be set to "InProgress" status if they have both `scheduledStartDate` and `scheduledEndDate` set.

**Example:**
```bash
# This will fail if task doesn't have time frame
curl -X PATCH http://localhost:5154/api/tasks/1/status \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '3'
```

### 5. Actual Time Tracking

When completing a task, the assignee must provide the actual start and end times they worked on it.

**Endpoint:**
- `POST /api/tasks/{id}/complete` - Complete a task with actual time

**Example:**
```bash
curl -X POST http://localhost:5154/api/tasks/1/complete \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "actualStartDate": "2025-10-12T09:30:00Z",
    "actualEndDate": "2025-10-12T16:45:00Z"
  }'
```

### 6. Approval Workflow

After a task is completed, it goes into "PendingApproval" status. The task owner (creator) must then approve or reject the completion.

**Endpoint:**
- `POST /api/tasks/{id}/approve-reject` - Approve or reject a completed task

**Workflow:**
1. Assignee completes task with actual time → Status: `PendingApproval`
2. Owner approves → Status: `Approved` (final)
3. Owner rejects → Status: `Assigned` (back to assignee)

**Example:**
```bash
# Approve task
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approve": true
  }'

# Reject task
curl -X POST http://localhost:5154/api/tasks/1/approve-reject \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approve": false,
    "rejectionReason": "Please fix the styling issues"
  }'
```

### 7. Admin User Management

Admins can now manage all users in the system.

**Endpoints:**
- `GET /api/admin/users` - Get all users
- `GET /api/admin/users/{userId}` - Get a specific user
- `DELETE /api/admin/users/{userId}` - Delete a user

**Example:**
```bash
curl -X GET http://localhost:5154/api/admin/users \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### 8. Unified Task View

**Endpoint:**
- `GET /api/tasks/all` - Get all tasks (created by or assigned to user)

This endpoint returns all tasks for the current user, suitable for displaying in a calendar or unified view.

## Updated Task Status Flow

The new task status workflow:

```
Pending (0) 
  ↓
Assigned (1)
  ↓
Accepted (2) or Rejected (7)
  ↓
InProgress (3) [requires time frame]
  ↓
PendingApproval (4) [requires actual time]
  ↓
Approved (6) or back to Assigned (1)
```

## Data Model Changes

### UserTask Model
Added fields:
- `ActualStartDate` - When the assignee actually started working
- `ActualEndDate` - When the assignee actually finished
- `Tags` - Collection of tags associated with the task
- `History` - Collection of history entries

### New Models
- `Tag` - Tag with name and color
- `TaskHistory` - History entry for task actions

## Breaking Changes

### TaskStatus Enum
New statuses added:
- `PendingApproval = 4`
- `Completed = 5` (renumbered from 4)
- `Approved = 6`
- `Rejected = 7` (renumbered from 5)

### AssignTaskDto
- `AssigneeEmail` is now optional (can be null to unassign)

### CreateTaskDto
Added fields:
- `TagIds` - List of tag IDs to associate with the task
- `AssigneeEmail` - Optional email to assign during creation

### UpdateTaskDto
Added fields:
- `TagIds` - List of tag IDs to associate with the task

### TaskResponseDto
Added fields:
- `ActualStartDate`
- `ActualEndDate`
- `Tags` - List of tags with their colors

## Testing the New Features

See the test script for examples of using all new features:

1. **Admin User Management** - List all users as admin
2. **Create Tags** - Create colored tags
3. **Create Task with Tags** - Create a task with tags and assignment
4. **Task History** - View complete history of task changes
5. **Business Rules** - Test InProgress requirement
6. **Complete Workflow** - Test the full approval workflow
7. **Unassignment** - Test unassigning tasks
8. **Calendar View** - Get all tasks for display

## Calendar Integration

The API now supports calendar-style views with:
- Scheduled time frames (`scheduledStartDate`, `scheduledEndDate`)
- Actual time frames (`actualStartDate`, `actualEndDate`)
- Tasks can have overlapping time frames (interfering tasks are allowed)
- Tag colors for visual categorization
- Unified endpoint to get all user's tasks

## Notes

- Tasks can have overlapping time frames (interfering tasks are allowed)
- Approved and Completed tasks cannot be modified or deleted
- Only task owners can approve/reject completed tasks
- Only assignees can complete tasks
- History is automatically tracked for all significant actions
