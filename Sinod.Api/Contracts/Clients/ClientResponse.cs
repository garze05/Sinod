namespace Sinod.Api.Contracts.Clients;

// Response DTO
public sealed record ClientResponse(
        int Id,
        string FirstName,
        string? LastName,
        string? Email,
        string Phone,
        int? CompanyId);
