using TimeManagement.Api.Models;

namespace TimeManagement.Api.DTOs;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public Models.TaskStatus Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatorEmail { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? AssigneeEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}
