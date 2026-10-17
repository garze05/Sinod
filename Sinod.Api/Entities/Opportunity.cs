using System.ComponentModel.DataAnnotations;
using Sinod.Api.Entities.ValueObjects;

namespace Sinod.Api.Entities;

public enum CommercialStatus
{
    Discovery, // We are getting to know the client
    QuoteInProgress, // We are preparing a Quotation doc for the client
    QuoteSent, // Quotation has been sent
    QuoteAccepted, // Pending payment (in case client is not municipality)
    Won, // Client paid, event is created
    Lost, // Client either told us no, or hasn't replied after the third follow-up attempt.
    
}

public class Opportunity
{
    public int Id { get; set; }
    
    [Required]
    [Range(1, int.MaxValue,  ErrorMessage = "Number of quotes must be greater than or equal to 1")]
    public int ClientId { get; set; }
    
    public OdClient? Client { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    #region Business specific
    
    public CommercialStatus Status { get; set; } = CommercialStatus.Discovery;
    public DateTime EventDate { get; set; }
    public EventLocation? Location { get; set; }
    public ICollection<OdService> Services { get; set; }
    
    #endregion
    
}