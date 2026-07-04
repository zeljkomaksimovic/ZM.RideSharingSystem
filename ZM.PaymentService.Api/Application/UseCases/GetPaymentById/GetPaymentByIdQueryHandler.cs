using MediatR;
using ZM.PaymentService.Api.Application.Repository;
using ZM.PaymentService.Api.Domain.ErrorMessages;
using ZM.PaymentService.Api.Domain.OperationResult;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentById
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<GetPaymentByIdDto>>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<GetPaymentByIdDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(request.PaymentId, cancellationToken);
            if (payment is null)
            {
                return Result<GetPaymentByIdDto>.Failure(Errors.Payment.PaymentNotFound());
            }

            var paymentDto = new GetPaymentByIdDto(
                payment.Id,
                payment.RideId, 
                payment.Amount,
                payment.Status,
                payment.CreatedAtUtc, 
                payment.ProcessedAtUtc);

            return Result<GetPaymentByIdDto>.Success(paymentDto);
        }
    }
}
