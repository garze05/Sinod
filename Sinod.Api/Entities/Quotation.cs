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
    
    public Opportunity Opportunity { get; set; }
    
    public QuotationState State { get; set; }
    
    
}