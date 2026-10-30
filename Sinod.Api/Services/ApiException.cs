using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Sinod.Api.Services;

public sealed class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public static ApiException NotFound(string resource) => new(404, $"{resource} was not found.");
    public static ApiException Invalid(string message) => new(400, message);
    public static ApiException Conflict(string message) => new(409, message);
}

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            ApiException error => (error.StatusCode, error.Message),
            DbUpdateConcurrencyException => (409, "The resource changed. Reload it before retrying."),
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" or "23503" } }
                => (409, "The write conflicts with an existing or referenced record. Reload before retrying."),
            _ => (500, "An unexpected error occurred.")
        };
        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new() { Status = status, Title = detail }
        });
    }
}
