namespace ZM.RideSharingSystem.Contracts.Events
{
    public record DriverAssignedEvent(Guid RideId, Guid DriverId);
}
