namespace ZM.MatchingService.Api.Domain.ValueObjects
{
    public record GeoLocation(double Latitude, double Longitude, string? Address = null);
}