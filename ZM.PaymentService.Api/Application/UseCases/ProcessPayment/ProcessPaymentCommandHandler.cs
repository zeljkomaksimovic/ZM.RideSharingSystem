using MassTransit;
using MediatR;
using ZM.PaymentService.Api.Application.DateTimeProvider;
using ZM.PaymentService.Api.Application.Repository;
using ZM.PaymentService.Api.Application.UnitOfWork;
using ZM.PaymentService.Api.Domain.Entities;
using ZM.PaymentService.Api.Domain.Events;
using ZM.PaymentService.Api.Domain.OperationResult;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.PaymentService.Api.Application.UseCases.ProcessPayment
{
    public class ProcessPaymentCommandHandler
        : IRequestHandler<ProcessPaymentCommand, Result>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProcessPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IPublishEndpoint publishEndpoint)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result> Handle(
            ProcessPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var payment = Payment.Create(
                Guid.NewGuid(),
                request.RideId,
                request.Amount,
                _dateTimeProvider.UtcNow);

            var completeResult = payment.Complete(_dateTimeProvider.UtcNow);
            if (completeResult.IsSuccessful is false)
            {
                return completeResult;
            }

            await _paymentRepository.CreatePaymentAsync(payment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}