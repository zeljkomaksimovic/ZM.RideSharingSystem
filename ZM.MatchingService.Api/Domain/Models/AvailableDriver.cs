namespace ZM.MatchingService.Api.Domain.Models
{
    public record AvailableDriver(Guid DriverId, double Latitude, double Longitude, DateTime LastLocationUpdateAtUtc);
}
