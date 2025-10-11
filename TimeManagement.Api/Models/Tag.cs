using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(7)] // Format: #RRGGBB
    public string Color { get; set; } = "#3498db"; // Default blue color
    
    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
}
