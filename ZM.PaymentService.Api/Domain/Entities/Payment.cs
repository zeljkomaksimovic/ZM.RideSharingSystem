using ZM.PaymentService.Api.Domain.Enums;
using ZM.PaymentService.Api.Domain.ErrorMessages;
using ZM.PaymentService.Api.Domain.Events;
using ZM.PaymentService.Api.Domain.OperationResult;
using ZM.PaymentService.Api.Domain.Primitives;

namespace ZM.PaymentService.Api.Domain.Entities
{
    public class Payment : AggregateRoot
    {
        private Payment(
            Guid id,
            Guid rideId,
            decimal amount,
            PaymentStatus status,
            DateTime createdAtUtc,
            DateTime? processedAtUtc)
        {
            Id = id;
            RideId = rideId;
            Amount = amount;
            Status = status;
            CreatedAtUtc = createdAtUtc;
            ProcessedAtUtc = processedAtUtc;
        }

        public Guid Id { get; private set; }
        public Guid RideId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }

        public static Payment Create(
            Guid id,
            Guid rideId,
            decimal amount,
            DateTime createdAtUtc)
        {
            return new Payment(
                id,
                rideId,
                amount,
                PaymentStatus.Pending,
                createdAtUtc,
                null);
        }

        public static Payment Rehydrate(
            Guid id,
            Guid rideId,
            decimal amount,
            PaymentStatus status,
            DateTime createdAtUtc,
            DateTime? processedAtUtc)
        {
            return new Payment(
                id,
                rideId,
                amount,
                status,
                createdAtUtc,
                processedAtUtc);
        }

        public Result Complete(string recipientEmail, DateTime processedAtUtc)
        {
            if (Status == PaymentStatus.Completed)
            {
                return Result.Failure(Errors.Payment.PaymentAlreadyCompleted());
            }

            Status = PaymentStatus.Completed;
            ProcessedAtUtc = processedAtUtc;

            Raise(new PaymentCompletedDomainEvent(Id, RideId, Amount, recipientEmail));

            return Result.Success();
        }

        public Result Fail(DateTime processedAtUtc)
        {
            if (Status == PaymentStatus.Failed)
            {
                return Result.Failure(Errors.Payment.PaymentFailed());
            }

            Status = PaymentStatus.Failed;
            ProcessedAtUtc = processedAtUtc;

            return Result.Success();
        }
    }
}