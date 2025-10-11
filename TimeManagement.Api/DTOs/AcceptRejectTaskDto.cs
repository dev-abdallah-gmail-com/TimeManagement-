using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class AcceptRejectTaskDto
{
    [Required]
    public bool Accept { get; set; }
    
    public string? RejectionReason { get; set; }
}
