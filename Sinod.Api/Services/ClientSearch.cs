using Sinod.Api.Entities;

namespace Sinod.Api.Services;

public static class ClientSearch
{
    public static IQueryable<OdClient> Apply(IQueryable<OdClient> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var term = search.Trim().ToLowerInvariant();
        var digits = new string(term.Where(char.IsAsciiDigit).ToArray());
        var phonePattern = string.Join("[^0-9]*", digits.ToCharArray());
        // Npgsql translates Regex.IsMatch to PostgreSQL's regex operator.
        return query.Where(c => (c.FirstName + " " + (c.LastName ?? "")).ToLower().Contains(term)
            || (digits.Length > 0 && System.Text.RegularExpressions.Regex.IsMatch(c.Phone, phonePattern))
            || (c.Company != null && c.Company.Name.ToLower().Contains(term)));
    }
}
