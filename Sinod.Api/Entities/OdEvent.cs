using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public class OdEvent
{
    public int Id { get; set; }
    
    [Required]
    [Range(1, int.MaxValue,  ErrorMessage = "You must select a service id")]
    public int OpportunityId { get; set; }
    
    [Required]
    public Opportunity Opportunity { get; set; } = null!;
    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CancellationReason { get; set; }
}

public enum EventStatus { Pending, Completed, Cancelled }
