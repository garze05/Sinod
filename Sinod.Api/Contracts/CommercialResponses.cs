using Sinod.Api.Contracts.Clients;
using Sinod.Api.Entities;
using Sinod.Api.Entities.ValueObjects;

namespace Sinod.Api.Contracts;

public sealed record CompanyResponse(int Id, string Name, string? Description, decimal BaseRate, CompanyType Type);
public sealed record PricingProfileResponse(int Id, ProfileType ProfileType, decimal BasePrice, int BaseDurationMinutes);
public sealed record ServiceResponse(int Id, string Name, string? Description, ServiceType ServiceType,
    int PricingProfileId, decimal? BasePrice);
public sealed record DepositResponse(int Id, decimal Amount, DateTime ReceivedAt, string Reference);
public sealed record QuotationItemResponse(int ServiceId, string ServiceName, int Quantity, int DurationMinutes,
    int BaseDurationMinutes, decimal UnitPrice, decimal DiscountPercent, decimal Subtotal, decimal Discount, decimal Total);
public sealed record QuotationResponse(int Id, int OpportunityId, int Version, QuotationState State,
    DateTime CreatedAt, DateTime? SentAt, DateTime? AcceptedAt, DateTime EventDate, string LocationSnapshot,
    string ClientSnapshot, bool IsMunicipal, decimal SurchargePercent, decimal Subtotal, decimal Discount,
    decimal Surcharge, decimal Transport, decimal Total, string Text, IReadOnlyList<QuotationItemResponse> Items);
public sealed record OpportunityResponse(int Id, ClientResponse Client, CommercialStatus Status,
    DateTime CreatedAt, DateTime UpdatedAt, DateTime? EventDate, EventLocation? Location,
    IReadOnlyList<int> ServiceIds, string? ClosureReason, IReadOnlyList<QuotationResponse> Quotations,
    IReadOnlyList<DepositResponse> Deposits, int? EventId);
public sealed record EventResponse(int Id, int OpportunityId, int? QuotationId, EventStatus Status,
    DateTime CreatedAt, DateTime? CompletedAt, string? CancellationReason, ClientResponse Client,
    DateTime EventDate, string LocationSnapshot, decimal Total);
