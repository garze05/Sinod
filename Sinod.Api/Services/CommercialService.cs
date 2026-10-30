using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sinod.Api.Contracts;
using Sinod.Api.Contracts.Clients;
using Sinod.Api.Data;
using Sinod.Api.Entities;
using static Sinod.Api.Services.CatalogService;

namespace Sinod.Api.Services;

public sealed class CommercialService(SinodDbContext db)
{
    public async Task<List<OpportunityResponse>> SearchAsync(string? search, int? clientId, CancellationToken ct)
    {
        var clients = ClientSearch.Apply(db.OdClients, search).Select(c => c.Id);
        var ids = await db.Opportunities.AsNoTracking()
            .Where(o => clients.Contains(o.ClientId) && (clientId == null || o.ClientId == clientId))
            .OrderByDescending(o => o.CreatedAt).Select(o => o.Id).ToListAsync(ct);
        var results = new List<OpportunityResponse>();
        foreach (var id in ids) results.Add(await GetAsync(id, ct));
        return results;
    }

    public async Task<OpportunityResponse> GetAsync(int id, CancellationToken ct)
    {
        var opportunity = await LoadAsync(id, ct);
        var quotes = await db.Quotations.AsNoTracking().Include(q => q.Items)
            .Where(q => q.OpportunityId == id).OrderBy(q => q.Version).ToListAsync(ct);
        var deposits = await db.Deposits.AsNoTracking().Where(d => d.OpportunityId == id).OrderBy(d => d.Id)
            .Select(d => new DepositResponse(d.Id, d.Amount, d.ReceivedAt, d.Reference)).ToListAsync(ct);
        var eventId = await db.OdEvents.Where(e => e.OpportunityId == id).Select(e => (int?)e.Id).SingleOrDefaultAsync(ct);
        return new(opportunity.Id, Client(opportunity.Client!), opportunity.Status, opportunity.CreatedAt,
            opportunity.UpdatedAt, opportunity.EventDate, opportunity.Location,
            opportunity.Services.Select(s => s.Id).Order().ToArray(), opportunity.ClosureReason,
            quotes.Select(ToResponse).ToList(), deposits, eventId);
    }

    public async Task<OpportunityResponse> SaveAsync(int? id, OpportunityRequest request, CancellationToken ct)
    {
        if (!await db.OdClients.AnyAsync(c => c.Id == request.ClientId, ct))
            throw ApiException.Invalid("The client does not exist.");
        if (request.EventDate is { } date) RequireUtc(date, "eventDate");
        if (request.ServiceIds is null) throw ApiException.Invalid("serviceIds cannot be null.");
        if (request.ServiceIds.Distinct().Count() != request.ServiceIds.Length)
            throw ApiException.Invalid("Service IDs must be unique.");
        ValidateLocation(request.Location);
        var services = await db.OdServices.Where(s => request.ServiceIds.Contains(s.Id)).ToListAsync(ct);
        if (services.Count != request.ServiceIds.Length) throw ApiException.Invalid("One or more services do not exist.");
        var opportunity = id is null ? new Opportunity { CreatedAt = DateTime.UtcNow } : await LoadAsync(id.Value, ct);
        RequireOpen(opportunity);
        if (opportunity.Status == CommercialStatus.QuoteAccepted)
            throw ApiException.Conflict("Accepted opportunities cannot be edited.");
        if (id is not null && opportunity.ClientId != request.ClientId
            && await db.Quotations.AnyAsync(q => q.OpportunityId == id, ct))
            throw ApiException.Conflict("The client cannot change after a quotation has been generated.");
        opportunity.ClientId = request.ClientId;
        opportunity.EventDate = request.EventDate;
        opportunity.Location = request.Location;
        opportunity.Services.Clear();
        foreach (var service in services) opportunity.Services.Add(service);
        Touch(opportunity);
        if (id is null) db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync(ct);
        // Load the selected client even when its FK was changed on a tracked entity.
        await db.Entry(opportunity).Reference(o => o.Client).LoadAsync(ct);
        return await GetAsync(opportunity.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var opportunity = await LoadAsync(id, ct);
        if (opportunity.Status != CommercialStatus.Discovery ||
            await db.Quotations.AnyAsync(q => q.OpportunityId == id, ct) ||
            await db.Deposits.AnyAsync(d => d.OpportunityId == id, ct) ||
            await db.OdEvents.AnyAsync(e => e.OpportunityId == id, ct))
            throw ApiException.Conflict("An opportunity with commercial history cannot be deleted. Record a loss instead.");
        opportunity.Services.Clear();
        db.Opportunities.Remove(opportunity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<QuotationResponse> QuoteAsync(int opportunityId, QuotationRequest request, CancellationToken ct)
    {
        var opportunity = await LoadAsync(opportunityId, ct);
        RequireOpen(opportunity);
        if (opportunity.Status == CommercialStatus.QuoteAccepted)
            throw ApiException.Conflict("An accepted quotation already exists.");
        if (opportunity.EventDate is not { } date || opportunity.Location is not { } location
            || string.IsNullOrWhiteSpace(location.Address))
            throw ApiException.Invalid("An event date and location address are required to quote.");
        if (request.Items.Any(i => i is null)) throw ApiException.Invalid("Quotation lines cannot be null.");
        if (request.Items.Select(i => i.ServiceId).Distinct().Count() != request.Items.Count)
            throw ApiException.Invalid("Combine repeated services into a single quotation line.");
        var serviceIds = request.Items.Select(i => i.ServiceId).ToArray();
        var services = await db.OdServices.Include(s => s.PricingProfile)
            .Where(s => serviceIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, ct);
        if (services.Count != serviceIds.Length) throw ApiException.Invalid("One or more services do not exist.");
        var quote = new Quotation
        {
            OpportunityId = opportunityId,
            Version = (await db.Quotations.Where(q => q.OpportunityId == opportunityId)
                .MaxAsync(q => (int?)q.Version, ct) ?? 0) + 1,
            CreatedAt = DateTime.UtcNow,
            EventDate = date,
            LocationSnapshot = JsonSerializer.Serialize(location),
            ClientSnapshot = JsonSerializer.Serialize(new
            {
                Client = Client(opportunity.Client!),
                Company = opportunity.Client!.Company is { } company
                    ? new CompanyResponse(company.Id, company.Name, company.Description, company.BaseRate, company.Type) : null
            }),
            IsMunicipal = opportunity.Client!.Company?.Type == CompanyType.Municipality,
            SurchargePercent = Money(opportunity.Client.Company?.BaseRate ?? 0),
            Transport = Money(request.Transport)
        };
        if (quote.SurchargePercent is < 0 or > 100)
            throw ApiException.Invalid("The company surcharge must be between 0 and 100 percent.");
        foreach (var line in request.Items)
        {
            var service = services[line.ServiceId];
            var profile = service.PricingProfile;
            if (profile.BaseDurationMinutes <= 0 || (service.BasePrice ?? profile.BasePrice) <= 0)
                throw ApiException.Invalid("The service must have a positive price and base duration.");
            if (line.DurationMinutes < profile.BaseDurationMinutes)
                throw ApiException.Invalid("Duration cannot be shorter than the service's base duration.");
            var unitPrice = Money(service.BasePrice ?? profile.BasePrice);
            var subtotal = Money(unitPrice * line.Quantity * line.DurationMinutes / profile.BaseDurationMinutes);
            var discountPercent = Money(line.DiscountPercent);
            var discount = Money(subtotal * discountPercent / 100);
            quote.Items.Add(new QuotationItem
            {
                ServiceId = service.Id, ServiceName = service.Name, Quantity = line.Quantity,
                DurationMinutes = line.DurationMinutes, BaseDurationMinutes = profile.BaseDurationMinutes,
                UnitPrice = unitPrice, DiscountPercent = discountPercent,
                Subtotal = subtotal, Discount = discount, Total = subtotal - discount
            });
        }
        quote.Subtotal = quote.Items.Sum(i => i.Subtotal);
        if (quote.Subtotal > 9999999999.99m)
            throw ApiException.Invalid("The quotation subtotal is outside the supported range.");
        quote.Discount = quote.Items.Sum(i => i.Discount);
        quote.Surcharge = Money((quote.Subtotal - quote.Discount) * quote.SurchargePercent / 100);
        quote.Total = quote.Subtotal - quote.Discount + quote.Surcharge + quote.Transport;
        if (quote.Total is <= 0 or > 9999999999.99m)
            throw ApiException.Invalid("The quotation total is outside the supported range.");
        var summary = string.Join("\n", quote.Items.Select(i =>
            FormattableString.Invariant($"{i.ServiceName}: {i.Quantity} x {i.DurationMinutes} min = CRC {i.Total:F2}")));
        quote.Text = string.IsNullOrWhiteSpace(request.Text)
            ? FormattableString.Invariant($"Cotización v{quote.Version}\nFecha: {date:O}\nLugar: {location.Address}\n{summary}\nRecargo: CRC {quote.Surcharge:F2}\nTransporte: CRC {quote.Transport:F2}\nTotal: CRC {quote.Total:F2}")
            : request.Text.Trim();
        opportunity.Status = CommercialStatus.QuoteInProgress;
        Touch(opportunity);
        db.Quotations.Add(quote);
        await db.SaveChangesAsync(ct);
        return ToResponse(quote);
    }

    public async Task<QuotationResponse> QuotationAsync(int opportunityId, int quoteId, CancellationToken ct) =>
        ToResponse(await LoadQuoteAsync(opportunityId, quoteId, ct));

    public async Task<QuotationResponse> SendAsync(int opportunityId, int quoteId, CancellationToken ct)
    {
        var opportunity = await LoadAsync(opportunityId, ct);
        RequireOpen(opportunity);
        var quote = await LoadQuoteAsync(opportunityId, quoteId, ct);
        RequireCompleteQuote(quote);
        if (opportunity.Status == CommercialStatus.QuoteAccepted)
            throw ApiException.Conflict("The opportunity already has an accepted quotation.");
        if (quote.State == QuotationState.Sent) return ToResponse(quote);
        if (quote.State != QuotationState.Draft) throw ApiException.Conflict("Only a draft can be marked as sent.");
        quote.State = QuotationState.Sent;
        quote.SentAt = DateTime.UtcNow;
        opportunity.Status = CommercialStatus.QuoteSent;
        Touch(opportunity);
        await db.SaveChangesAsync(ct);
        return ToResponse(quote);
    }

    public async Task<QuotationResponse> AcceptAsync(int opportunityId, int quoteId, CancellationToken ct)
    {
        var opportunity = await LoadAsync(opportunityId, ct);
        RequireOpen(opportunity);
        var quote = await LoadQuoteAsync(opportunityId, quoteId, ct);
        RequireCompleteQuote(quote);
        if (quote.State == QuotationState.Accepted) return ToResponse(quote);
        if (quote.State != QuotationState.Sent) throw ApiException.Conflict("Only a sent quotation can be accepted.");
        if (await db.Quotations.AnyAsync(q => q.OpportunityId == opportunityId && q.State == QuotationState.Accepted, ct))
            throw ApiException.Conflict("Another version has already been accepted.");
        quote.State = QuotationState.Accepted;
        quote.AcceptedAt = DateTime.UtcNow;
        opportunity.Status = CommercialStatus.QuoteAccepted;
        Touch(opportunity);
        await db.SaveChangesAsync(ct);
        return ToResponse(quote);
    }

    public async Task<DepositResponse> DepositAsync(int opportunityId, DepositRequest request, CancellationToken ct)
    {
        var opportunity = await LoadAsync(opportunityId, ct);
        RequireOpen(opportunity);
        var quote = await AcceptedAsync(opportunityId, ct);
        if (quote.IsMunicipal) throw ApiException.Conflict("Municipal contracts do not require or register a deposit.");
        RequireUtc(request.ReceivedAt, "receivedAt");
        if (request.ReceivedAt > DateTime.UtcNow) throw ApiException.Invalid("A received deposit cannot have a future date.");
        if (Money(request.Amount) != request.Amount) throw ApiException.Invalid("Amounts support at most two decimal places.");
        var alreadyReceived = await db.Deposits.Where(d => d.OpportunityId == opportunityId).SumAsync(d => d.Amount, ct);
        if (alreadyReceived + request.Amount > quote.Total) throw ApiException.Invalid("Deposits cannot exceed the accepted total.");
        var deposit = new Deposit { OpportunityId = opportunityId, Amount = request.Amount,
            ReceivedAt = request.ReceivedAt, Reference = request.Reference.Trim() };
        db.Deposits.Add(deposit);
        Touch(opportunity);
        await db.SaveChangesAsync(ct);
        return new(deposit.Id, deposit.Amount, deposit.ReceivedAt, deposit.Reference);
    }

    public async Task<EventResponse> WinAsync(int opportunityId, CloseOpportunityRequest request, CancellationToken ct)
    {
        var opportunity = await LoadAsync(opportunityId, ct);
        var existing = await db.OdEvents.Where(e => e.OpportunityId == opportunityId).Select(e => (int?)e.Id).SingleOrDefaultAsync(ct);
        if (existing is { } existingId) return await EventAsync(existingId, ct);
        RequireOpen(opportunity);
        var quote = await AcceptedAsync(opportunityId, ct);
        if (!request.AvailabilityConfirmed) throw ApiException.Invalid("Confirm resource availability before closing.");
        var received = await db.Deposits.Where(d => d.OpportunityId == opportunityId).SumAsync(d => d.Amount, ct);
        // Round up to avoid accepting less than 50% on totals with an odd cent.
        var required = decimal.Ceiling(quote.Total * 50) / 100;
        if (!quote.IsMunicipal && received < required)
            throw ApiException.Conflict($"A deposit of at least {required.ToString("F2", CultureInfo.InvariantCulture)} is required.");
        opportunity.Status = CommercialStatus.Won;
        opportunity.ClosureReason = request.Reason.Trim();
        Touch(opportunity);
        var result = new OdEvent { OpportunityId = opportunityId, QuotationId = quote.Id, CreatedAt = DateTime.UtcNow };
        db.OdEvents.Add(result);
        // One atomic save: closing and event creation succeed or fail together.
        await db.SaveChangesAsync(ct);
        return await EventAsync(result.Id, ct);
    }

    public async Task<OpportunityResponse> LoseAsync(int id, ReasonRequest request, CancellationToken ct)
    {
        var opportunity = await LoadAsync(id, ct);
        RequireOpen(opportunity);
        if (await db.Deposits.AnyAsync(d => d.OpportunityId == id, ct))
            throw ApiException.Conflict("An opportunity with received deposits requires payment resolution before recording a loss.");
        opportunity.Status = CommercialStatus.Lost;
        opportunity.ClosureReason = request.Reason.Trim();
        Touch(opportunity);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task<List<EventResponse>> EventsAsync(string? search, int? clientId, CancellationToken ct)
    {
        var clients = ClientSearch.Apply(db.OdClients, search).Select(c => c.Id);
        var ids = await db.OdEvents.Where(e => clients.Contains(e.Opportunity.ClientId)
            && (clientId == null || e.Opportunity.ClientId == clientId))
            .OrderBy(e => e.Id).Select(e => e.Id).ToListAsync(ct);
        var results = new List<EventResponse>();
        foreach (var id in ids) results.Add(await EventAsync(id, ct));
        return results;
    }

    public async Task<EventResponse> EventAsync(int id, CancellationToken ct)
    {
        var result = await db.OdEvents.Include(e => e.Opportunity).ThenInclude(o => o.Client)
            .Include(e => e.Quotation).SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw ApiException.NotFound("Event");
        return new(result.Id, result.OpportunityId, result.QuotationId, result.Status, result.CreatedAt,
            result.CompletedAt, result.CancellationReason, Client(result.Opportunity.Client!),
            result.Quotation?.EventDate ?? result.Opportunity.EventDate ?? default,
            result.Quotation?.LocationSnapshot ?? JsonSerializer.Serialize(result.Opportunity.Location),
            result.Quotation?.Total ?? 0);
    }

    public async Task<EventResponse> UpdateEventAsync(int id, EventUpdateRequest request, CancellationToken ct)
    {
        var result = await db.OdEvents.FindAsync([id], ct) ?? throw ApiException.NotFound("Event");
        if (result.Status != EventStatus.Pending && result.Status != request.Status)
            throw ApiException.Conflict("Completed or cancelled events cannot change state.");
        if (request.Status == EventStatus.Cancelled && string.IsNullOrWhiteSpace(request.CancellationReason))
            throw ApiException.Invalid("A cancellation reason is required.");
        result.Status = request.Status;
        result.CompletedAt = request.Status == EventStatus.Completed ? result.CompletedAt ?? DateTime.UtcNow : null;
        result.CancellationReason = request.Status == EventStatus.Cancelled ? request.CancellationReason!.Trim() : null;
        await db.SaveChangesAsync(ct);
        return await EventAsync(id, ct);
    }

    private async Task<Opportunity> LoadAsync(int id, CancellationToken ct) =>
        await db.Opportunities.Include(o => o.Client).ThenInclude(c => c!.Company).Include(o => o.Services)
            .SingleOrDefaultAsync(o => o.Id == id, ct) ?? throw ApiException.NotFound("Opportunity");
    private async Task<Quotation> LoadQuoteAsync(int opportunityId, int id, CancellationToken ct) =>
        await db.Quotations.Include(q => q.Items).SingleOrDefaultAsync(q => q.Id == id && q.OpportunityId == opportunityId, ct)
        ?? throw ApiException.NotFound("Quotation");
    private async Task<Quotation> AcceptedAsync(int opportunityId, CancellationToken ct)
    {
        var quote = await db.Quotations.Include(q => q.Items)
            .SingleOrDefaultAsync(q => q.OpportunityId == opportunityId && q.State == QuotationState.Accepted, ct)
            ?? throw ApiException.Conflict("Accept a sent quotation before proceeding.");
        RequireCompleteQuote(quote);
        return quote;
    }
    private static void RequireOpen(Opportunity opportunity)
    {
        if (opportunity.Status is CommercialStatus.Won or CommercialStatus.Lost)
            throw ApiException.Conflict("The opportunity is closed.");
    }
    private static void Touch(Opportunity opportunity) => opportunity.UpdatedAt = DateTime.UtcNow;
    private static void RequireCompleteQuote(Quotation quote)
    {
        if (quote.Items.Count == 0 || quote.Total <= 0)
            throw ApiException.Conflict("This legacy quotation has no economic snapshot. Generate a complete version before proceeding.");
    }
    private static void RequireUtc(DateTime date, string field)
    {
        if (date == default || date.Kind != DateTimeKind.Utc) throw ApiException.Invalid($"{field} must be a UTC timestamp ending in Z.");
    }
    private static void ValidateLocation(Entities.ValueObjects.EventLocation? location)
    {
        if (location?.Coordinates is { } coordinates &&
            (!double.IsFinite(coordinates.Latitude) || !double.IsFinite(coordinates.Longitude)
             || coordinates.Latitude is < -90 or > 90 || coordinates.Longitude is < -180 or > 180))
            throw ApiException.Invalid("Coordinates are outside their valid ranges.");
    }
    private static ClientResponse Client(OdClient client) =>
        new(client.Id, client.FirstName, client.LastName, client.Email, client.Phone, client.CompanyId);
    private static QuotationResponse ToResponse(Quotation q) => new(q.Id, q.OpportunityId, q.Version, q.State,
        q.CreatedAt, q.SentAt, q.AcceptedAt, q.EventDate, q.LocationSnapshot, q.ClientSnapshot, q.IsMunicipal,
        q.SurchargePercent, q.Subtotal, q.Discount, q.Surcharge, q.Transport, q.Total, q.Text,
        q.Items.OrderBy(i => i.Id).Select(i => new QuotationItemResponse(i.ServiceId, i.ServiceName, i.Quantity,
            i.DurationMinutes, i.BaseDurationMinutes, i.UnitPrice, i.DiscountPercent, i.Subtotal, i.Discount, i.Total)).ToList());
}
