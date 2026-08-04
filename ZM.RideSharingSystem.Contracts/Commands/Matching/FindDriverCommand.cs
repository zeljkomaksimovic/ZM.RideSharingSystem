namespace ZM.RideSharingSystem.Contracts.Commands.Matching
{
    public record FindDriverCommand(Guid RideId, double Latitude, double Longitude);
}
