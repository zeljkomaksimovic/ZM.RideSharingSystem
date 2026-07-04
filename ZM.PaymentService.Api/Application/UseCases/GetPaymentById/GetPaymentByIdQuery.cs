using MediatR;
using ZM.PaymentService.Api.Domain.OperationResult;

namespace ZM.PaymentService.Api.Application.UseCases.GetPaymentById
{
    public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<Result<GetPaymentByIdDto>>;
}
