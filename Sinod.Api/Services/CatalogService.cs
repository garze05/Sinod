using Microsoft.EntityFrameworkCore;
using Sinod.Api.Contracts;
using Sinod.Api.Data;
using Sinod.Api.Entities;

namespace Sinod.Api.Services;

public sealed class CatalogService(SinodDbContext db)
{
    public async Task<List<CompanyResponse>> CompaniesAsync(CancellationToken ct) =>
        await db.Companies.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CompanyResponse(x.Id, x.Name, x.Description, x.BaseRate, x.Type)).ToListAsync(ct);

    public async Task<CompanyResponse> CompanyAsync(int id, CancellationToken ct) =>
        ToResponse(await db.Companies.FindAsync([id], ct) ?? throw ApiException.NotFound("Company"));

    public async Task<CompanyResponse> SaveCompanyAsync(int? id, CompanyRequest request, CancellationToken ct)
    {
        var company = id is null ? new Company() : await db.Companies.FindAsync([id.Value], ct)
            ?? throw ApiException.NotFound("Company");
        company.Name = request.Name.Trim();
        company.Description = request.Description?.Trim();
        company.BaseRate = Money(request.BaseRate);
        company.Type = request.Type;
        if (id is null) db.Companies.Add(company);
        await db.SaveChangesAsync(ct);
        return ToResponse(company);
    }

    public async Task DeleteCompanyAsync(int id, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([id], ct) ?? throw ApiException.NotFound("Company");
        if (await db.OdClients.AnyAsync(x => x.CompanyId == id, ct))
            throw ApiException.Conflict("The company is referenced by clients.");
        db.Companies.Remove(company);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<PricingProfileResponse>> ProfilesAsync(CancellationToken ct) =>
        await db.OdServicePricingProfiles.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new PricingProfileResponse(x.Id, x.ProfileType, x.BasePrice, x.BaseDurationMinutes)).ToListAsync(ct);

    public async Task<PricingProfileResponse> ProfileAsync(int id, CancellationToken ct) =>
        ToResponse(await db.OdServicePricingProfiles.FindAsync([id], ct) ?? throw ApiException.NotFound("Pricing profile"));

    public async Task<PricingProfileResponse> SaveProfileAsync(int? id, PricingProfileRequest request, CancellationToken ct)
    {
        var profile = id is null ? new OdServicePricingProfile() : await db.OdServicePricingProfiles.FindAsync([id.Value], ct)
            ?? throw ApiException.NotFound("Pricing profile");
        profile.ProfileType = request.ProfileType;
        profile.BasePrice = Money(request.BasePrice);
        profile.BaseDurationMinutes = request.BaseDurationMinutes;
        if (id is null) db.OdServicePricingProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return ToResponse(profile);
    }

    public async Task DeleteProfileAsync(int id, CancellationToken ct)
    {
        var profile = await db.OdServicePricingProfiles.FindAsync([id], ct) ?? throw ApiException.NotFound("Pricing profile");
        if (await db.OdServices.AnyAsync(x => x.PricingProfileId == id, ct))
            throw ApiException.Conflict("The pricing profile is referenced by services.");
        db.OdServicePricingProfiles.Remove(profile);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<ServiceResponse>> ServicesAsync(CancellationToken ct) =>
        await db.OdServices.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new ServiceResponse(x.Id, x.Name, x.Description, x.ServiceType, x.PricingProfileId, x.BasePrice)).ToListAsync(ct);

    public async Task<ServiceResponse> ServiceAsync(int id, CancellationToken ct) =>
        ToResponse(await db.OdServices.FindAsync([id], ct) ?? throw ApiException.NotFound("Service"));

    public async Task<ServiceResponse> SaveServiceAsync(int? id, ServiceRequest request, CancellationToken ct)
    {
        if (!await db.OdServicePricingProfiles.AnyAsync(x => x.Id == request.PricingProfileId, ct))
            throw ApiException.Invalid("The pricing profile does not exist.");
        var service = id is null ? new OdService() : await db.OdServices.FindAsync([id.Value], ct)
            ?? throw ApiException.NotFound("Service");
        service.Name = request.Name.Trim();
        service.Description = request.Description?.Trim();
        service.ServiceType = request.ServiceType;
        service.PricingProfileId = request.PricingProfileId;
        service.BasePrice = request.BasePrice is { } price ? Money(price) : null;
        if (id is null) db.OdServices.Add(service);
        await db.SaveChangesAsync(ct);
        return ToResponse(service);
    }

    public async Task DeleteServiceAsync(int id, CancellationToken ct)
    {
        var service = await db.OdServices.FindAsync([id], ct) ?? throw ApiException.NotFound("Service");
        if (await db.Opportunities.AnyAsync(x => x.Services.Any(s => s.Id == id), ct))
            throw ApiException.Conflict("The service is referenced by opportunities.");
        db.OdServices.Remove(service);
        await db.SaveChangesAsync(ct);
    }

    public static decimal Money(decimal amount) => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    private static CompanyResponse ToResponse(Company x) => new(x.Id, x.Name, x.Description, x.BaseRate, x.Type);
    private static PricingProfileResponse ToResponse(OdServicePricingProfile x) => new(x.Id, x.ProfileType, x.BasePrice, x.BaseDurationMinutes);
    private static ServiceResponse ToResponse(OdService x) => new(x.Id, x.Name, x.Description, x.ServiceType, x.PricingProfileId, x.BasePrice);
}
