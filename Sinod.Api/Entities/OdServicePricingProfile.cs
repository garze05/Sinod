using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sinod.Api.Entities;

public enum ProfileType
{
    StandardCharacter,
    RentedCharacter,
    SpecialCharacter,
    SmallInflatable,
    BigInflatable,
}

public class OdServicePricingProfile
{
    public int Id { get; set; }
    
    public ProfileType ProfileType { get; set; }
    
    [Precision(12, 2)]
    public decimal BasePrice { get; set; }
    
    // Need to define if this is required
    // public int MinimumDurationMinutes { get; set; }
    
    public int BaseDurationMinutes { get; set; }
}