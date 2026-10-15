using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public enum CompanyType
{
    Kindergarten,
    School,
    HighSchool,
    University,
    Mall,
    Association,
    Municipality,
}

public class Company
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name maximum length is 100")]
    public string Name { get; set; } =  string.Empty;
    
    [StringLength(254, ErrorMessage = "Description maximum length is 254")]
    public string? Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Base Rate is required")]
    public decimal BaseRate { get; set; }
    
    public CompanyType Type { get; set; }
}