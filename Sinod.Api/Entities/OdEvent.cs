using System.ComponentModel.DataAnnotations;

namespace Sinod.Api.Entities;

public class OdEvent
{
    public int Id { get; set; }
    
    [Required]
    [Range(1, int.MaxValue,  ErrorMessage = "You must select a service id")]
    public int OpportunityId { get; set; }
    
    [Required]
    public Opportunity Opportunity { get; set; }
}