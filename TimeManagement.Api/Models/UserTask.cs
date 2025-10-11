using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.Models;

public class UserTask
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? ScheduledStartDate { get; set; }
    
    public DateTime? ScheduledEndDate { get; set; }
    
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    
    public ApplicationUser? Creator { get; set; }
    
    public string? AssignedTo { get; set; }
    
    public ApplicationUser? Assignee { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? RejectionReason { get; set; }
}
