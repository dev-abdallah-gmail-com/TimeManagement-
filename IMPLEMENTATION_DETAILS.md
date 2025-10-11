# Implementation Details - Enhancements

This document provides technical details about the implementation of the new features.

## Summary of Changes

### Models Added/Modified

#### New Models
1. **Tag.cs**
   - Properties: Id, Name, Color
   - Many-to-many relationship with UserTask

2. **TaskHistory.cs**
   - Properties: Id, TaskId, Action, PerformedBy, Details, OldStatus, NewStatus, PerformedAt
   - Tracks all task-related actions

#### Modified Models
1. **UserTask.cs**
   - Added: ActualStartDate, ActualEndDate
   - Added: Tags collection (many-to-many)
   - Added: History collection (one-to-many)

2. **TaskStatus.cs** (enum)
   - Added: PendingApproval (4)
   - Renumbered: Completed (5), Approved (6), Rejected (7)

### DTOs Added/Modified

#### New DTOs
1. **TagDto.cs** - For tag responses
2. **CreateTagDto.cs** - For creating tags
3. **TaskHistoryDto.cs** - For history responses
4. **CompleteTaskDto.cs** - For completing tasks with actual time
5. **ApproveRejectTaskDto.cs** - For approving/rejecting tasks
6. **UserDto.cs** - For user information (admin)

#### Modified DTOs
1. **CreateTaskDto.cs**
   - Added: TagIds (List<int>)
   - Added: AssigneeEmail (string?)

2. **UpdateTaskDto.cs**
   - Added: TagIds (List<int>)

3. **TaskResponseDto.cs**
   - Added: ActualStartDate, ActualEndDate
   - Added: Tags (List<TagDto>)

4. **AssignTaskDto.cs**
   - Changed: AssigneeEmail from required to optional (nullable)

### Services

#### New Services
1. **IUserService.cs / UserService.cs**
   - GetAllUsersAsync() - Get all users with roles
   - GetUserByIdAsync(userId) - Get specific user
   - DeleteUserAsync(userId) - Delete user

2. **ITagService.cs / TagService.cs**
   - GetAllTagsAsync() - List all tags
   - GetTagByIdAsync(id) - Get specific tag
   - CreateTagAsync(dto) - Create new tag
   - UpdateTagAsync(id, dto) - Update tag
   - DeleteTagAsync(id) - Delete tag

#### Modified Services
1. **ITaskService.cs / TaskService.cs**
   - Modified AssignTaskAsync to accept nullable email
   - Added CompleteTaskAsync(id, dto, userId)
   - Added ApproveRejectTaskAsync(id, dto, userId)
   - Added GetTaskHistoryAsync(id, userId)
   - Added GetAllTasksAsync(userId)
   - Added private AddHistoryEntry() method
   - Updated all methods to include Tags in queries
   - Updated all methods to track history
   - Enhanced UpdateTaskStatusAsync with time frame validation

### Controllers

#### New Controllers
1. **AdminController.cs**
   - GET /api/admin/users - List all users
   - GET /api/admin/users/{userId} - Get user
   - DELETE /api/admin/users/{userId} - Delete user
   - Requires Admin role

2. **TagsController.cs**
   - GET /api/tags - List all tags
   - GET /api/tags/{id} - Get tag
   - POST /api/tags - Create tag
   - PUT /api/tags/{id} - Update tag
   - DELETE /api/tags/{id} - Delete tag
   - Requires authentication

#### Modified Controllers
1. **TasksController.cs**
   - Added POST /api/tasks/{id}/complete - Complete task with actual time
   - Added POST /api/tasks/{id}/approve-reject - Approve/reject task
   - Added GET /api/tasks/{id}/history - Get task history
   - Added GET /api/tasks/all - Get all tasks (calendar view)
   - Enhanced error messages for status updates

### Database Schema Changes

#### New Tables
1. **Tags**
   - Id (INTEGER, PRIMARY KEY)
   - Name (TEXT, NOT NULL)
   - Color (TEXT, NOT NULL)

2. **TaskHistories**
   - Id (INTEGER, PRIMARY KEY)
   - TaskId (INTEGER, FOREIGN KEY)
   - Action (TEXT, NOT NULL)
   - PerformedBy (TEXT, FOREIGN KEY)
   - Details (TEXT)
   - OldStatus (INTEGER)
   - NewStatus (INTEGER)
   - PerformedAt (TEXT, NOT NULL)

3. **TagUserTask** (Junction table)
   - TagsId (INTEGER, FOREIGN KEY)
   - TasksId (INTEGER, FOREIGN KEY)

#### Modified Tables
1. **Tasks**
   - Added: ActualStartDate (TEXT)
   - Added: ActualEndDate (TEXT)

### Business Logic

#### Key Business Rules Implemented

1. **InProgress Status Validation**
   ```csharp
   // In UpdateTaskStatusAsync
   if (status == Models.TaskStatus.InProgress)
   {
       if (!task.ScheduledStartDate.HasValue || !task.ScheduledEndDate.HasValue)
       {
           return null; // Cannot set to InProgress without time frame
       }
   }
   ```

2. **Task Completion with Actual Time**
   ```csharp
   // In CompleteTaskAsync
   task.ActualStartDate = dto.ActualStartDate;
   task.ActualEndDate = dto.ActualEndDate;
   task.Status = Models.TaskStatus.PendingApproval;
   ```

3. **Approval Workflow**
   ```csharp
   // In ApproveRejectTaskAsync
   if (dto.Approve)
   {
       task.Status = Models.TaskStatus.Approved;
       task.CompletedAt = DateTime.UtcNow;
   }
   else
   {
       task.Status = Models.TaskStatus.Assigned;
       task.RejectionReason = dto.RejectionReason;
   }
   ```

4. **Flexible Assignment**
   ```csharp
   // In AssignTaskAsync
   if (string.IsNullOrEmpty(assigneeEmail))
   {
       // Unassign
       task.AssignedTo = null;
       task.Status = Models.TaskStatus.Pending;
   }
   else
   {
       // Assign or self-assign
       var assignee = await _userManager.FindByEmailAsync(assigneeEmail);
       task.AssignedTo = assignee!.Id;
       task.Status = Models.TaskStatus.Assigned;
   }
   ```

5. **History Tracking**
   ```csharp
   // Automatically called for all actions
   private async Task AddHistoryEntry(int taskId, string action, string userId, 
       string? details, TaskStatus? oldStatus, TaskStatus? newStatus)
   {
       var history = new TaskHistory
       {
           TaskId = taskId,
           Action = action,
           PerformedBy = userId,
           Details = details,
           OldStatus = oldStatus,
           NewStatus = newStatus,
           PerformedAt = DateTime.UtcNow
       };
       _context.TaskHistories.Add(history);
       await _context.SaveChangesAsync();
   }
   ```

### Authorization

#### Role-Based Access Control

1. **Admin-Only Endpoints**
   - AdminController requires `[Authorize(Roles = "Admin")]`
   - Only users with Admin role can access user management

2. **Task Permissions**
   - Task owner can: update, delete, assign, approve/reject
   - Task assignee can: accept/reject, complete, update status
   - Others: no access

3. **Task Visibility**
   - Users can only see tasks they created or are assigned to
   - History is only accessible to task owner and assignee

### Data Validation

#### Tag Validation
- Name: Required, max 50 characters
- Color: Required, max 7 characters, must match #RRGGBB format

#### Task Completion Validation
- ActualStartDate: Required
- ActualEndDate: Required
- Only assignee can complete
- Task must be in InProgress or Accepted status

#### Approval Validation
- Only task owner (creator) can approve/reject
- Task must be in PendingApproval status

### API Response Patterns

#### Success Responses
```json
// Task with tags and times
{
  "id": 1,
  "title": "Fix bug",
  "scheduledStartDate": "2025-10-12T09:00:00Z",
  "scheduledEndDate": "2025-10-12T17:00:00Z",
  "actualStartDate": "2025-10-12T09:15:00Z",
  "actualEndDate": "2025-10-12T16:30:00Z",
  "status": 6,
  "tags": [
    {"id": 1, "name": "Bug", "color": "#e74c3c"}
  ]
}
```

#### Error Responses
```json
// Business rule violation
{
  "message": "Failed to update task status. Task not found, you don't have permission, or business rules prevented the status change (e.g., InProgress requires time frame)."
}
```

### Performance Considerations

1. **Eager Loading**
   - Tags, Creator, and Assignee are included in queries
   - Prevents N+1 query problems

2. **History Optimization**
   - History is only loaded when explicitly requested
   - Ordered by PerformedAt descending for recent-first display

3. **Tag Management**
   - Many-to-many relationship handled efficiently by EF Core
   - Tags are reusable across tasks

### Testing Coverage

All features tested with manual API calls:
- ✅ User registration and authentication
- ✅ Admin user management (list, get, delete)
- ✅ Tag CRUD operations
- ✅ Task creation with tags and assignment
- ✅ Self-assignment and unassignment
- ✅ Status validation (InProgress requires time frame)
- ✅ Task completion with actual time
- ✅ Approval workflow (approve and reject)
- ✅ Task history tracking
- ✅ Calendar view (all tasks)
- ✅ Overlapping tasks support
- ✅ Permission enforcement

### Migration Path

The application uses SQLite with EF Core's EnsureCreated():
1. Delete old database file
2. Run application
3. Database is recreated with new schema
4. Roles are seeded automatically

For production with existing data:
1. Use EF Core migrations instead of EnsureCreated()
2. Create migration: `dotnet ef migrations add AddEnhancements`
3. Update database: `dotnet ef database update`

## Key Technical Decisions

1. **Status Enum Changes**
   - Renumbered existing statuses to accommodate new ones
   - Breaking change for existing APIs

2. **Nullable AssigneeEmail**
   - Allows unassignment by passing null/empty
   - Backward compatible (empty object `{}` works)

3. **Separate Complete and Approve**
   - Two-step process for better control
   - PendingApproval intermediate status

4. **History as Separate Table**
   - Better for querying and auditing
   - Can grow independently

5. **Tags Many-to-Many**
   - Reusable across tasks
   - Efficient storage

6. **Self-Assignment Allowed**
   - Flexible workflow
   - Users can pick up unassigned tasks

## Future Enhancements

Potential improvements not in current scope:
- Task comments/discussions
- File attachments
- Task dependencies
- Recurring tasks
- Email notifications
- Task templates
- Time tracking with pause/resume
- Advanced filtering and search
- Bulk operations
- Task export (CSV, PDF)
