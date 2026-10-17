using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(50, ErrorMessage = "Email must be 30 characters max")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(30, ErrorMessage = "Name must be 30 characters max")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(30, ErrorMessage = "Last Name must be 30 characters max")]
    public string LastName { get; set; } =  string.Empty;
    
    // We would need to add a secure password field.
    // Probably with ASP.NET Core Identity
}