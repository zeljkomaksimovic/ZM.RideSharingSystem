using MediatR;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendRideCancelledNotification
{
    public record SendRideCancelledNotificationCommand(Guid RideId, string RecipientEmail) : IRequest<Result>;
}
