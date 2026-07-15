using ZM.PaymentService.Api.Domain.Primitives;

namespace ZM.PaymentService.Api.Domain.Events
{
    public record PaymentCompletedDomainEvent(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail) : IDomainEvent;
}
