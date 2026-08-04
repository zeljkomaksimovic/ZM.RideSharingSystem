namespace ZM.RideSharingSystem.Contracts.Events
{
    public record RideCreatedEvent(Guid RideId, double Latitude, double Longitude, string RecipientEmail);
}
