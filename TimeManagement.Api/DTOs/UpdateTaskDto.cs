using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class UpdateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? ScheduledStartDate { get; set; }
    
    public DateTime? ScheduledEndDate { get; set; }
    
    public List<int> TagIds { get; set; } = new List<int>();
}
