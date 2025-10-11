using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class CompleteTaskDto
{
    [Required]
    public DateTime ActualStartDate { get; set; }
    
    [Required]
    public DateTime ActualEndDate { get; set; }
}
