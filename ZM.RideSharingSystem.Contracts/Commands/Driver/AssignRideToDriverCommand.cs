namespace ZM.RideSharingSystem.Contracts.Commands.Driver
{
    public record AssignRideToDriverCommand(Guid RideId, Guid DriverId);
}
