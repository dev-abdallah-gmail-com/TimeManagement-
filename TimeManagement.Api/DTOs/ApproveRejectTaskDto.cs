using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class ApproveRejectTaskDto
{
    [Required]
    public bool Approve { get; set; }
    
    [MaxLength(1000)]
    public string? RejectionReason { get; set; }
}
