namespace ZM.RideSharingSystem.Contracts.Events
{
    public record DriverLocationUpdatedEvent(Guid DriverId, double Latitude, double Longitude);
}
