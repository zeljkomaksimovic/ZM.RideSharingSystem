using MediatR;
using ZM.PaymentService.Api.Application.Repository;
using ZM.PaymentService.Api.Domain.ErrorMessages;
using ZM.PaymentService.Api.Domain.OperationResult;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentByRideId
{
    public class GetPaymentByRideIdQueryHandler : IRequestHandler<GetPaymentByRideIdQuery, Result<GetPaymentByRideIdDto>>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByRideIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<GetPaymentByRideIdDto>> Handle(GetPaymentByRideIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByRideIdAsync(request.RideId, cancellationToken);
            if (payment is null)
            {
                return Result<GetPaymentByRideIdDto>.Failure(Errors.Payment.PaymentNotFound());
            }

            var paymentDto = new GetPaymentByRideIdDto(
                payment.Id,
                payment.RideId, 
                payment.Amount, 
                payment.Status, 
                payment.CreatedAtUtc, 
                payment.ProcessedAtUtc);

            return Result<GetPaymentByRideIdDto>.Success(paymentDto);
        }
    }
}
