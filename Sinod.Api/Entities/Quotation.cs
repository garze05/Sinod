using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public enum QuotationState
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired
}

public class Quotation
{
    public int Id { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Number must be greater than or equal to 1")]
    public int OpportunityId { get; set; }
    
    public Opportunity Opportunity { get; set; } = null!;
    
    public QuotationState State { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime EventDate { get; set; }
    public string LocationSnapshot { get; set; } = string.Empty;
    public string ClientSnapshot { get; set; } = string.Empty;
    public bool IsMunicipal { get; set; }
    public decimal SurchargePercent { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Surcharge { get; set; }
    public decimal Transport { get; set; }
    public decimal Total { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<QuotationItem> Items { get; set; } = [];
}

public class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    // Snapshot: intentionally no FK to the mutable service catalogue.
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int DurationMinutes { get; set; }
    public int BaseDurationMinutes { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}
