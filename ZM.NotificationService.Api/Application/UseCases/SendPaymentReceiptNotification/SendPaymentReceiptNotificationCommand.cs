using MediatR;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendPaymentReceiptNotification
{
    public record SendPaymentReceiptNotificationCommand(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail) : IRequest<Result>;
}
