namespace ZM.RideSharingSystem.Contracts.Commands.Notification
{
    public record RideCompletedNotificationCommand(Guid RideId, string RecipientEmail);
}
