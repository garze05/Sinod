using Sinod.Api.Contracts.Clients;

namespace Sinod.Api.Services;

public enum ClientWriteError
{
    ClientNotFound,
    CompanyNotFound
}

public sealed record ClientWriteResult(
    ClientResponse? Client,
    ClientWriteError? Error = null);