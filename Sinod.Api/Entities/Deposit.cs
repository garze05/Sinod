namespace Sinod.Api.Entities;

public class Deposit
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Reference { get; set; } = string.Empty;
}
