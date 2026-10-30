using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Contracts.Clients;

// Request DTO
public sealed class ClientDetailsRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "The name can't be longer than 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Last name can't be longer than 100 characters.")]
    public string? LastName { get; set; }

    [EmailAddress(ErrorMessage = "Email is invalid")]
    [StringLength(254, ErrorMessage = "The Email can't be longer than 254 characters.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Phone number is invalid")]
    [StringLength(30, ErrorMessage = "Phone number cannot be longer than 30 characters")]
    public string Phone { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Company Id is not valid")]
    public int? CompanyId { get; set; }
}