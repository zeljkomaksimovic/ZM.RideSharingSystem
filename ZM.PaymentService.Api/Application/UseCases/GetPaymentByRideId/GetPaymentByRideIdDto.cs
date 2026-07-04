using ZM.PaymentService.Api.Domain.Enums;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentByRideId
{
    public record GetPaymentByRideIdDto(
        Guid Id,
        Guid RideId,
        decimal Amount,
        PaymentStatus Status,
        DateTime CreatedAtUtc,
        DateTime? ProcessedAtUtc);
}
