using MediatR;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendDriverAssignedNotification
{
    public class SendDriverAssignedNotificationCommandHandler : IRequestHandler<SendDriverAssignedNotificationCommand, Result>
    {
        private readonly IEmailSender _emailSender;

        public SendDriverAssignedNotificationCommandHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(SendDriverAssignedNotificationCommand request, CancellationToken cancellationToken)
        {
            await _emailSender.SendEmailAsync(
                request.RecipientEmail,
                EmailTemplates.Driver.AssignedSubject,
                EmailTemplates.Driver.Assigned(request.RideId, request.DriverId),
                cancellationToken);

            return Result.Success();
        }
    }
}