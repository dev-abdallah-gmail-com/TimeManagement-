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

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
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
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only creator or assignee can update if not completed
        if (task.CreatedBy != userId && task.AssignedTo != userId)
            return null;

        if (task.Status == Models.TaskStatus.Completed)
            return null;

        task.Title = taskDto.Title;
        task.Description = taskDto.Description;
        task.ScheduledStartDate = taskDto.ScheduledStartDate;
        task.ScheduledEndDate = taskDto.ScheduledEndDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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

        // Cannot delete completed tasks
        if (task.Status == Models.TaskStatus.Completed)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TaskResponseDto?> AssignTaskAsync(int taskId, string assigneeEmail, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
            return null;

        // Only creator can assign tasks
        if (task.CreatedBy != userId)
            return null;

        // Cannot assign completed tasks
        if (task.Status == Models.TaskStatus.Completed)
            return null;

        var assignee = await _userManager.FindByEmailAsync(assigneeEmail);
        if (assignee == null)
            return null;

        task.AssignedTo = assignee.Id;
        task.Status = Models.TaskStatus.Assigned;
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

        if (dto.Accept)
        {
            task.Status = Models.TaskStatus.Accepted;
            task.RejectionReason = null;
        }
        else
        {
            task.Status = Models.TaskStatus.Rejected;
            task.RejectionReason = dto.RejectionReason;
            task.AssignedTo = null; // Unassign the task
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

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        if (status == Models.TaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await MapToResponseDto(task);
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
            RejectionReason = task.RejectionReason
        };
    }
}
