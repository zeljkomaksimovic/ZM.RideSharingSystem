namespace ZM.RideSharingSystem.Contracts.Commands.Notification
{
    public record PaymentReceiptNotificationCommand(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail);
}
