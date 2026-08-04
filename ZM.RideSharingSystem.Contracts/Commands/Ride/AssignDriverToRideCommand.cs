namespace ZM.RideSharingSystem.Contracts.Commands.Ride
{
    public record AssignDriverToRideCommand(Guid RideId, Guid DriverId);
}
