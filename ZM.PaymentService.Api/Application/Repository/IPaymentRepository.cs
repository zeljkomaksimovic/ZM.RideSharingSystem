using ZM.PaymentService.Api.Domain.Entities;

namespace ZM.PaymentService.Api.Application.Repository
{
    public interface IPaymentRepository
    {
        Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
        Task<Payment?> GetPaymentByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default);
        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
