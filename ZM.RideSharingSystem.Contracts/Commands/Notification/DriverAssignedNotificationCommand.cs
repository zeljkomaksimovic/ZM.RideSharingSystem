namespace ZM.RideSharingSystem.Contracts.Commands.Notification
{
    public record DriverAssignedNotificationCommand(Guid RideId, Guid DriverId, string RecipientEmail);
}
