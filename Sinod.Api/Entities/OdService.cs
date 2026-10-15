using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sinod.Api.Entities;

public enum ServiceType
{
    Character,
    Inflatable,
    FacePainter,
    Entertainer, // Animation
    Other
}

public class OdService
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(30, ErrorMessage = "Name must be 30 characters max")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(254, ErrorMessage = "Description must be 254 characters max")]
    public string? Description { get; set; } = string.Empty;
    
    public ServiceType ServiceType { get; set; }
    
    public int PricingProfileId { get; set; }
    
    public OdServicePricingProfile PricingProfile { get; set; }
    
    // If and only if this specific service needs a specific price
    [Precision(12, 2)]
    public decimal? BasePrice { get; set; }
}