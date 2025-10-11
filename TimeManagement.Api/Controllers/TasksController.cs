using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;
using TimeManagement.Api.Services;

namespace TimeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskDto taskDto)
    {
        var userId = GetUserId();
        var result = await _taskService.CreateTaskAsync(taskDto, userId);
        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
    {
        var userId = GetUserId();
        var result = await _taskService.GetTaskByIdAsync(id, userId);

        if (result == null)
        {
            return NotFound(new { message = "Task not found or you don't have access to it." });
        }

        return Ok(result);
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<List<TaskResponseDto>>> GetMyTasks()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetMyTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpGet("assigned-to-me")]
    public async Task<ActionResult<List<TaskResponseDto>>> GetAssignedTasks()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAssignedTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto)
    {
        var userId = GetUserId();
        var result = await _taskService.UpdateTaskAsync(id, taskDto, userId);

        if (result == null)
        {
            return NotFound(new { message = "Task not found, you don't have permission, or task is completed." });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        var result = await _taskService.DeleteTaskAsync(id, userId);

        if (!result)
        {
            return NotFound(new { message = "Task not found, you don't have permission, or task is completed." });
        }

        return NoContent();
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult<TaskResponseDto>> AssignTask(int id, [FromBody] AssignTaskDto assignDto)
    {
        var userId = GetUserId();
        var result = await _taskService.AssignTaskAsync(id, assignDto.AssigneeEmail, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Failed to assign task. Task not found, you don't have permission, assignee doesn't exist, or task is completed." });
        }

        return Ok(result);
    }

    [HttpPost("{id}/accept-reject")]
    public async Task<ActionResult<TaskResponseDto>> AcceptRejectTask(int id, [FromBody] AcceptRejectTaskDto dto)
    {
        var userId = GetUserId();
        var result = await _taskService.AcceptRejectTaskAsync(id, dto, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Failed to accept/reject task. Task not found, you're not the assignee, or task is not in assigned status." });
        }

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTaskStatus(int id, [FromBody] Models.TaskStatus status)
    {
        var userId = GetUserId();
        var result = await _taskService.UpdateTaskStatusAsync(id, status, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Failed to update task status. Task not found, you don't have permission, or business rules prevented the status change (e.g., InProgress requires time frame)." });
        }

        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<TaskResponseDto>> CompleteTask(int id, [FromBody] CompleteTaskDto dto)
    {
        var userId = GetUserId();
        var result = await _taskService.CompleteTaskAsync(id, dto, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Failed to complete task. Task not found, you're not the assignee, or task is not in correct status." });
        }

        return Ok(result);
    }

    [HttpPost("{id}/approve-reject")]
    public async Task<ActionResult<TaskResponseDto>> ApproveRejectTask(int id, [FromBody] ApproveRejectTaskDto dto)
    {
        var userId = GetUserId();
        var result = await _taskService.ApproveRejectTaskAsync(id, dto, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Failed to approve/reject task. Task not found, you're not the owner, or task is not pending approval." });
        }

        return Ok(result);
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<TaskHistoryDto>>> GetTaskHistory(int id)
    {
        var userId = GetUserId();
        var history = await _taskService.GetTaskHistoryAsync(id, userId);
        return Ok(history);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<TaskResponseDto>>> GetAllTasks()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllTasksAsync(userId);
        return Ok(tasks);
    }
}
