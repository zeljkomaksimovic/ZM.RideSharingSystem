namespace ZM.RideSharingSystem.Contracts.Events
{
    public record DriverMatchedEvent(Guid RideId, Guid DriverId);
}
