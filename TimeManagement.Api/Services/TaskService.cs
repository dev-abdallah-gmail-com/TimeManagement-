using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeManagement.Api.Data;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto taskDto, string userId)
    {
        var task = new UserTask
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            ScheduledStartDate = taskDto.ScheduledStartDate,
            ScheduledEndDate = taskDto.ScheduledEndDate,
            CreatedBy = userId,
            Status = Models.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add tags if provided
        if (taskDto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => taskDto.TagIds.Contains(t.Id)).ToListAsync();
            task.Tags = tags;
        }

        // Handle assignment if assigneeEmail is provided
        if (!string.IsNullOrEmpty(taskDto.AssigneeEmail))
        {
            var assignee = await _userManager.FindByEmailAsync(taskDto.AssigneeEmail);
            if (assignee != null)
            {
                task.AssignedTo = assignee.Id;
                task.Status = Models.TaskStatus.Assigned;
            }
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Add history entry
        await AddHistoryEntry(task.Id, "Created", userId, $"Task created: {task.Title}", null, task.Status);

        if (!string.IsNullOrEmpty(task.AssignedTo))
        {
            await AddHistoryEntry(task.Id, "Assigned", userId, $"Task assigned to {taskDto.AssigneeEmail}", null, null);
        }

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return null;

        // User can only see tasks they created or are assigned to
        if (task.CreatedBy != userId && task.AssignedTo != userId)
            return null;

        return await MapToResponseDto(task);
    }

    public async Task<List<TaskResponseDto>> GetMyTasksAsync(string userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .Where(t => t.CreatedBy == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responseDtos = new List<TaskResponseDto>();
        foreach (var task in tasks)
        {
            responseDtos.Add(await MapToResponseDto(task));
        }

        return responseDtos;
    }

    public async Task<List<TaskResponseDto>> GetAssignedTasksAsync(string userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .Where(t => t.AssignedTo == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responseDtos = new List<TaskResponseDto>();
        foreach (var task in tasks)
        {
            responseDtos.Add(await MapToResponseDto(task));
        }

        return responseDtos;
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(int taskId, UpdateTaskDto taskDto, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return null;

        // Only creator or assignee can update if not completed
        if (task.CreatedBy != userId && task.AssignedTo != userId)
            return null;

        if (task.Status == Models.TaskStatus.Completed || task.Status == Models.TaskStatus.Approved)
            return null;

        task.Title = taskDto.Title;
        task.Description = taskDto.Description;
        task.ScheduledStartDate = taskDto.ScheduledStartDate;
        task.ScheduledEndDate = taskDto.ScheduledEndDate;
        task.UpdatedAt = DateTime.UtcNow;

        // Update tags
        task.Tags.Clear();
        if (taskDto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => taskDto.TagIds.Contains(t.Id)).ToListAsync();
            foreach (var tag in tags)
            {
                task.Tags.Add(tag);
            }
        }

        await _context.SaveChangesAsync();

        await AddHistoryEntry(taskId, "Updated", userId, "Task details updated", null, null);

        return await MapToResponseDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int taskId, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return false;

        // Only creator can delete
        if (task.CreatedBy != userId)
            return false;

        // Cannot delete completed or approved tasks
        if (task.Status == Models.TaskStatus.Completed || task.Status == Models.TaskStatus.Approved)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TaskResponseDto?> AssignTaskAsync(int taskId, string? assigneeEmail, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // User can assign to themselves or creator can assign to anyone
        bool isSelfAssignment = false;
        if (!string.IsNullOrEmpty(assigneeEmail))
        {
            var assignee = await _userManager.FindByEmailAsync(assigneeEmail);
            if (assignee == null)
                return null;
            
            isSelfAssignment = assignee.Id == userId;
        }

        // Only creator or the user themselves can assign
        if (task.CreatedBy != userId && !isSelfAssignment)
            return null;

        // Cannot assign completed or approved tasks
        if (task.Status == Models.TaskStatus.Completed || task.Status == Models.TaskStatus.Approved)
            return null;

        var oldAssignee = task.AssignedTo;

        if (string.IsNullOrEmpty(assigneeEmail))
        {
            // Unassign the task
            task.AssignedTo = null;
            task.Status = Models.TaskStatus.Pending;
            await AddHistoryEntry(taskId, "Unassigned", userId, "Task unassigned", null, null);
        }
        else
        {
            var assignee = await _userManager.FindByEmailAsync(assigneeEmail);
            task.AssignedTo = assignee!.Id;
            task.Status = Models.TaskStatus.Assigned;
            
            var action = oldAssignee == null ? "Assigned" : "Reassigned";
            await AddHistoryEntry(taskId, action, userId, $"Task assigned to {assigneeEmail}", null, null);
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> AcceptRejectTaskAsync(int taskId, AcceptRejectTaskDto dto, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only assignee can accept/reject
        if (task.AssignedTo != userId)
            return null;

        // Can only accept/reject tasks in Assigned status
        if (task.Status != Models.TaskStatus.Assigned)
            return null;

        var oldStatus = task.Status;

        if (dto.Accept)
        {
            task.Status = Models.TaskStatus.Accepted;
            task.RejectionReason = null;
            await AddHistoryEntry(taskId, "StatusChanged", userId, "Task accepted by assignee", oldStatus, task.Status);
        }
        else
        {
            task.Status = Models.TaskStatus.Rejected;
            task.RejectionReason = dto.RejectionReason;
            task.AssignedTo = null; // Unassign the task
            await AddHistoryEntry(taskId, "StatusChanged", userId, $"Task rejected: {dto.RejectionReason}", oldStatus, task.Status);
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateTaskStatusAsync(int taskId, Models.TaskStatus status, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only assignee or creator can update status
        if (task.CreatedBy != userId && task.AssignedTo != userId)
            return null;

        // Business rule: Can't set to InProgress without time frame
        if (status == Models.TaskStatus.InProgress)
        {
            if (!task.ScheduledStartDate.HasValue || !task.ScheduledEndDate.HasValue)
            {
                return null; // Cannot set to InProgress without time frame
            }
        }

        var oldStatus = task.Status;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntry(taskId, "StatusChanged", userId, $"Status changed from {oldStatus} to {status}", oldStatus, status);

        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> CompleteTaskAsync(int taskId, CompleteTaskDto dto, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only assignee can complete the task
        if (task.AssignedTo != userId)
            return null;

        // Task must be in InProgress or Accepted status
        if (task.Status != Models.TaskStatus.InProgress && task.Status != Models.TaskStatus.Accepted)
            return null;

        var oldStatus = task.Status;
        task.ActualStartDate = dto.ActualStartDate;
        task.ActualEndDate = dto.ActualEndDate;
        task.Status = Models.TaskStatus.PendingApproval;
        task.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntry(taskId, "StatusChanged", userId, 
            $"Task completed and pending approval. Actual time: {dto.ActualStartDate:g} - {dto.ActualEndDate:g}", 
            oldStatus, task.Status);

        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> ApproveRejectTaskAsync(int taskId, ApproveRejectTaskDto dto, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only task owner (creator) can approve/reject
        if (task.CreatedBy != userId)
            return null;

        // Task must be in PendingApproval status
        if (task.Status != Models.TaskStatus.PendingApproval)
            return null;

        var oldStatus = task.Status;

        if (dto.Approve)
        {
            task.Status = Models.TaskStatus.Approved;
            task.CompletedAt = DateTime.UtcNow;
            task.RejectionReason = null;
            await AddHistoryEntry(taskId, "Approved", userId, "Task approved by owner", oldStatus, task.Status);
        }
        else
        {
            task.Status = Models.TaskStatus.Assigned;
            task.RejectionReason = dto.RejectionReason;
            await AddHistoryEntry(taskId, "Rejected", userId, $"Task rejected by owner: {dto.RejectionReason}", oldStatus, task.Status);
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<List<TaskHistoryDto>> GetTaskHistoryAsync(int taskId, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return new List<TaskHistoryDto>();

        // User can only see history of tasks they created or are assigned to
        if (task.CreatedBy != userId && task.AssignedTo != userId)
            return new List<TaskHistoryDto>();

        var history = await _context.TaskHistories
            .Include(h => h.PerformedByUser)
            .Where(h => h.TaskId == taskId)
            .OrderByDescending(h => h.PerformedAt)
            .ToListAsync();

        return history.Select(h => new TaskHistoryDto
        {
            Id = h.Id,
            TaskId = h.TaskId,
            Action = h.Action,
            PerformedBy = h.PerformedBy,
            PerformedByEmail = h.PerformedByUser?.Email ?? "",
            Details = h.Details,
            OldStatus = h.OldStatus,
            NewStatus = h.NewStatus,
            PerformedAt = h.PerformedAt
        }).ToList();
    }

    public async Task<List<TaskResponseDto>> GetAllTasksAsync(string userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .Where(t => t.CreatedBy == userId || t.AssignedTo == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responseDtos = new List<TaskResponseDto>();
        foreach (var task in tasks)
        {
            responseDtos.Add(await MapToResponseDto(task));
        }

        return responseDtos;
    }

    private async Task AddHistoryEntry(int taskId, string action, string userId, string? details, 
        Models.TaskStatus? oldStatus, Models.TaskStatus? newStatus)
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

    private async Task<TaskResponseDto> MapToResponseDto(UserTask task)
    {
        var creator = await _userManager.FindByIdAsync(task.CreatedBy);
        ApplicationUser? assignee = null;
        
        if (!string.IsNullOrEmpty(task.AssignedTo))
        {
            assignee = await _userManager.FindByIdAsync(task.AssignedTo);
        }

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ScheduledStartDate = task.ScheduledStartDate,
            ScheduledEndDate = task.ScheduledEndDate,
            Status = task.Status,
            CreatedBy = task.CreatedBy,
            CreatorEmail = creator?.Email ?? "",
            AssignedTo = task.AssignedTo,
            AssigneeEmail = assignee?.Email,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            RejectionReason = task.RejectionReason,
            ActualStartDate = task.ActualStartDate,
            ActualEndDate = task.ActualEndDate,
            Tags = task.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList()
        };
    }
}
