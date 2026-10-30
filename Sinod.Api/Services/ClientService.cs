using Microsoft.EntityFrameworkCore;
using Sinod.Api.Contracts.Clients;
using Sinod.Api.Data;
using Sinod.Api.Entities;

namespace Sinod.Api.Services;

public class ClientService(SinodDbContext dbContext)
{
    public async Task<List<ClientResponse>> SearchAsync(string? search, CancellationToken ct)
    {
        var query = ClientSearch.Apply(dbContext.OdClients.AsNoTracking(), search);
        return await query.OrderBy(x => x.Id)
            .Select(x => new ClientResponse(x.Id, x.FirstName, x.LastName, x.Email, x.Phone, x.CompanyId))
            .ToListAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var client = await dbContext.OdClients.FindAsync([id], ct) ?? throw ApiException.NotFound("Client");
        if (await dbContext.Opportunities.AnyAsync(o => o.ClientId == id, ct))
            throw ApiException.Conflict("The client has commercial history.");
        dbContext.OdClients.Remove(client);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<ClientResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var client = await dbContext.OdClients
            .AsNoTracking() //Read-only query
            .SingleOrDefaultAsync(
                client => client.Id == id,
                cancellationToken);

        return client is null ? null : ToResponse(client);
    }

    public async Task<ClientWriteResult> CreateAsync(
        ClientDetailsRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CompanyExistsAsync(request.CompanyId, cancellationToken))
        {
            return new(null, ClientWriteError.CompanyNotFound);
        }

        var client = new OdClient();
        ApplyDetails(client, request);

        dbContext.OdClients.Add(client);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new(ToResponse(client));
    }

    public async Task<ClientWriteResult> UpdateAsync(
        int id,
        ClientDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var client = await dbContext.OdClients
            .SingleOrDefaultAsync(
                client => client.Id == id,
                cancellationToken);

        if (client is null)
        {
            return new(null, ClientWriteError.ClientNotFound);
        }

        if (!await CompanyExistsAsync(request.CompanyId, cancellationToken))
        {
            return new(null, ClientWriteError.CompanyNotFound);
        }

        ApplyDetails(client, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new(ToResponse(client));
    }

    private async Task<bool> CompanyExistsAsync(
        int? companyId,
        CancellationToken cancellationToken)
    {
        return companyId is null ||
            await dbContext.Companies.AnyAsync(
                company => company.Id == companyId,
                cancellationToken);
    }

    private static void ApplyDetails(
        OdClient client,
        ClientDetailsRequest request)
    {
        client.FirstName = request.FirstName.Trim();
        client.LastName = request.LastName?.Trim();
        client.Email = request.Email?.Trim();
        client.Phone = request.Phone.Trim();
        client.CompanyId = request.CompanyId;
    }

    private static ClientResponse ToResponse(OdClient client)
    {
        return new ClientResponse(
            client.Id,
            client.FirstName,
            client.LastName,
            client.Email,
            client.Phone,
            client.CompanyId);
    }
}
