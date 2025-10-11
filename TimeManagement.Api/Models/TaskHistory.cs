using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.Models;

public class TaskHistory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int TaskId { get; set; }
    
    public UserTask? Task { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Created, Updated, Assigned, StatusChanged, Approved, Rejected
    
    [Required]
    public string PerformedBy { get; set; } = string.Empty;
    
    public ApplicationUser? PerformedByUser { get; set; }
    
    [MaxLength(1000)]
    public string? Details { get; set; }
    
    public TaskStatus? OldStatus { get; set; }
    
    public TaskStatus? NewStatus { get; set; }
    
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}
