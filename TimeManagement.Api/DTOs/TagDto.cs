using System.ComponentModel.DataAnnotations;

namespace TimeManagement.Api.DTOs;

public class TagDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in format #RRGGBB")]
    public string Color { get; set; } = "#3498db";
}

public class CreateTagDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in format #RRGGBB")]
    public string Color { get; set; } = "#3498db";
}
