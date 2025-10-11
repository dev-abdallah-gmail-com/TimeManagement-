using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Services;

public interface ITaskService
{
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto taskDto, string userId);
    Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, string userId);
    Task<List<TaskResponseDto>> GetMyTasksAsync(string userId);
    Task<List<TaskResponseDto>> GetAssignedTasksAsync(string userId);
    Task<TaskResponseDto?> UpdateTaskAsync(int taskId, UpdateTaskDto taskDto, string userId);
    Task<bool> DeleteTaskAsync(int taskId, string userId);
    Task<TaskResponseDto?> AssignTaskAsync(int taskId, string assigneeEmail, string userId);
    Task<TaskResponseDto?> AcceptRejectTaskAsync(int taskId, AcceptRejectTaskDto dto, string userId);
    Task<TaskResponseDto?> UpdateTaskStatusAsync(int taskId, Models.TaskStatus status, string userId);
}
