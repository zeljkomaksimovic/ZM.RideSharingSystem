using MediatR;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendRideCompletedNotification
{
    public record SendRideCompletedNotificationCommand(Guid RideId, string RecipientEmail) : IRequest<Result>;
}
