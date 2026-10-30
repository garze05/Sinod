using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public class OdClient
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "First Name cannot be longer than 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Last Name cannot be longer than 100 characters")]
    public string? LastName { get; set; }
    
    [EmailAddress(ErrorMessage = "Email is invalid")]
    [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Phone number is invalid")]
    [StringLength(30, ErrorMessage = "Phone number cannot be longer than 30 characters")]
    public string Phone { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue, ErrorMessage = "Company Id is not valid")]
    public int? CompanyId { get; set; }
    
    public Company? Company { get; set; }
    
    
}