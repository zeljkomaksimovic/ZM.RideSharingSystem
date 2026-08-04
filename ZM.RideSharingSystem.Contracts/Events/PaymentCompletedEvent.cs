namespace ZM.RideSharingSystem.Contracts.Events
{
    public record PaymentCompletedEvent(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail);
}
