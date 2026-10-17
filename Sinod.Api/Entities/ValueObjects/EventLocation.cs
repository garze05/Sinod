using System.ComponentModel.DataAnnotations.Schema;

namespace Sinod.Api.Entities.ValueObjects;

// Record -> A type designed to hold data. Two records are considered equal when their values match.
// Sealed -> Prevents other types from inheriting from it.
public sealed record EventLocation()
{
    public string? Address { get; init; }
    public string? Province { get; init; }
    public string? Directions { get; init; }
    public string? GooglePlaceId { get; init; }
    public GeoPoint? Coordinates { get; init; }
}