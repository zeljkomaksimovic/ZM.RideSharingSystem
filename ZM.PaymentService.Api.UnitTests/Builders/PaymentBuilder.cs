using ZM.PaymentService.Api.Domain.Entities;
using ZM.PaymentService.Api.Domain.Enums;

namespace ZM.PaymentService.Api.UnitTests.Builders
{
    internal static class PaymentBuilder
    {
        public static Payment BuildPending(
            Guid? id = null,
            Guid? rideId = null,
            decimal amount = 10m,
            DateTime? createdAt = null)
        {
            return Payment.Rehydrate(
                id ?? Guid.NewGuid(),
                rideId ?? Guid.NewGuid(),
                amount,
                PaymentStatus.Pending,
                createdAt ?? DateTime.UtcNow,
                processedAtUtc: null);
        }

        public static Payment BuildCompleted(
            Guid? id = null,
            Guid? rideId = null,
            decimal amount = 10m,
            DateTime? createdAt = null,
            DateTime? processedAt = null)
        {
            return Payment.Rehydrate(
                id ?? Guid.NewGuid(),
                rideId ?? Guid.NewGuid(),
                amount,
                PaymentStatus.Completed,
                createdAt ?? DateTime.UtcNow.AddMinutes(-5),
                processedAt ?? DateTime.UtcNow);
        }

        public static Payment BuildFailed(
            Guid? id = null,
            Guid? rideId = null,
            decimal amount = 10m,
            DateTime? createdAt = null,
            DateTime? processedAt = null)
        {
            return Payment.Rehydrate(
                id ?? Guid.NewGuid(),
                rideId ?? Guid.NewGuid(),
                amount,
                PaymentStatus.Failed,
                createdAt ?? DateTime.UtcNow.AddMinutes(-5),
                processedAt ?? DateTime.UtcNow);
        }
    }
}