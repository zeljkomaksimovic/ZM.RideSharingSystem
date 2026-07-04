using MediatR;
using ZM.PaymentService.Api.Domain.OperationResult;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentByRideId
{
    public record GetPaymentByRideIdQuery(Guid RideId) : IRequest<Result<GetPaymentByRideIdDto>>;
}
