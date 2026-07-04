using Microsoft.EntityFrameworkCore;
using ZM.PaymentService.Api.Application.Repository;
using ZM.PaymentService.Api.Domain.Entities;

namespace ZM.PaymentService.Api.Persistence.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _dbContext;

        public PaymentRepository(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var dbPayment = new Models.Payment
            {
                Id = payment.Id,
                RideId = payment.RideId,
                Amount = payment.Amount,
                Status = ZM.PaymentService.Api.Domain.Enums.PaymentStatus.Pending,
                CreatedAtUtc = payment.CreatedAtUtc
            };

            await _dbContext.Payments.AddAsync(dbPayment, cancellationToken);
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _dbContext.Payments
                .Where(p => p.Id == paymentId)
                .Select(p => Payment.Rehydrate(
                    p.Id,
                    p.RideId,
                    p.Amount,
                    p.Status,
                    p.CreatedAtUtc,
                    p.CompletedAtUtc))
                .FirstOrDefaultAsync(cancellationToken);

            return payment;
        }

        public async Task<Payment?> GetPaymentByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default)
        {
            var payment = await _dbContext.Payments
                .Where(p => p.RideId == rideId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => Payment.Rehydrate(
                    p.Id,
                    p.RideId,
                    p.Amount,
                    p.Status,
                    p.CreatedAtUtc,
                    p.CompletedAtUtc))
                .FirstOrDefaultAsync(cancellationToken);

            return payment;
        }

        public async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var dbPayment = await _dbContext.Payments
                .Where(p => p.Id == payment.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (dbPayment is null)
            { 
                return;
            }

            dbPayment.Amount = payment.Amount;
            dbPayment.Status = payment.Status;
            dbPayment.CompletedAtUtc = payment.ProcessedAtUtc;

            _dbContext.Update(dbPayment);
        }
    }
}
