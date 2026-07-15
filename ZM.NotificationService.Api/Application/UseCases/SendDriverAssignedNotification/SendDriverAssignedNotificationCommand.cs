using MediatR;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendDriverAssignedNotification
{
    public record SendDriverAssignedNotificationCommand(Guid RideId, Guid DriverId, string RecipientEmail) : IRequest<Result>;
}
