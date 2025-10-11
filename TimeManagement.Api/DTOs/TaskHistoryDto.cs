using TimeManagement.Api.Models;

namespace TimeManagement.Api.DTOs;

public class TaskHistoryDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string PerformedByEmail { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Models.TaskStatus? OldStatus { get; set; }
    public Models.TaskStatus? NewStatus { get; set; }
    public DateTime PerformedAt { get; set; }
}
