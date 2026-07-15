using MediatR;
using ZM.PaymentService.Api.Domain.OperationResult;

namespace ZM.PaymentService.Api.Application.UseCases.ProcessPayment
{
    public record ProcessPaymentCommand(Guid RideId, decimal Amount, string RecipientEmail) : IRequest<Result>;
}
