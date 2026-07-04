using ZM.PaymentService.Api.Domain.Enums;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentById
{
    public record GetPaymentByIdDto(
        Guid Id,
        Guid RideId,
        decimal Amount,
        PaymentStatus Status,
        DateTime CreatedAtUtc,
        DateTime? ProcessedAtUtc);
}
