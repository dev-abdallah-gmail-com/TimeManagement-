using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class AssignTaskDto
{
    [Required]
    public string AssigneeEmail { get; set; } = string.Empty;
}
