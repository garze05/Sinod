using System.ComponentModel.DataAnnotations;
using Sinod.Api.Entities;
using Sinod.Api.Entities.ValueObjects;

namespace Sinod.Api.Contracts;

public sealed class CompanyRequest
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [StringLength(254)] public string? Description { get; set; }
    // Percentage surcharge, e.g. 5 means 5%.
    [Range(0, 100)] public decimal BaseRate { get; set; }
    [EnumDataType(typeof(CompanyType))] public CompanyType Type { get; set; }
}

public sealed class PricingProfileRequest
{
    [EnumDataType(typeof(ProfileType))] public ProfileType ProfileType { get; set; }
    [Range(typeof(decimal), "0.01", "9999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal BasePrice { get; set; }
    [Range(1, 1440)] public int BaseDurationMinutes { get; set; }
}

public sealed class ServiceRequest
{
    [Required, StringLength(30)] public string Name { get; set; } = string.Empty;
    [StringLength(254)] public string? Description { get; set; }
    [EnumDataType(typeof(ServiceType))] public ServiceType ServiceType { get; set; }
    [Range(1, int.MaxValue)] public int PricingProfileId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal? BasePrice { get; set; }
}

public sealed class OpportunityRequest
{
    [Range(1, int.MaxValue)] public int ClientId { get; set; }
    public DateTime? EventDate { get; set; }
    public EventLocation? Location { get; set; }
    [MaxLength(100)] public int[] ServiceIds { get; set; } = [];
}

public sealed class QuotationRequest
{
    [Required, MinLength(1), MaxLength(100)] public List<QuotationItemRequest> Items { get; set; } = [];
    [Range(typeof(decimal), "0", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Transport { get; set; }
    [StringLength(20000)] public string? Text { get; set; }
}

public sealed class QuotationItemRequest
{
    [Range(1, int.MaxValue)] public int ServiceId { get; set; }
    [Range(1, 1000)] public int Quantity { get; set; } = 1;
    [Range(1, 1440)] public int DurationMinutes { get; set; }
    [Range(0, 30)] public decimal DiscountPercent { get; set; }
}

public sealed class DepositRequest
{
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Amount { get; set; }
    public DateTime ReceivedAt { get; set; }
    [Required, StringLength(200)] public string Reference { get; set; } = string.Empty;
}

public sealed class CloseOpportunityRequest
{
    public bool AvailabilityConfirmed { get; set; }
    [Required, StringLength(1000)] public string Reason { get; set; } = string.Empty;
}

public sealed class ReasonRequest
{
    [Required, StringLength(1000)] public string Reason { get; set; } = string.Empty;
}

public sealed class EventUpdateRequest
{
    [EnumDataType(typeof(EventStatus))] public EventStatus Status { get; set; }
    [StringLength(1000)] public string? CancellationReason { get; set; }
}
