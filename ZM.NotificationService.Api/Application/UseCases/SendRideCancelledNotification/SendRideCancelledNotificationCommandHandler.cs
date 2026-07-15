using MediatR;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendRideCancelledNotification
{
    public class SendRideCancelledNotificationCommandHandler : IRequestHandler<SendRideCancelledNotificationCommand, Result>
    {
        private readonly IEmailSender _emailSender;

        public SendRideCancelledNotificationCommandHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(SendRideCancelledNotificationCommand request, CancellationToken cancellationToken)
        {
            await _emailSender.SendEmailAsync(
                request.RecipientEmail,
                EmailTemplates.Ride.CancelledSubject,
                EmailTemplates.Ride.Cancelled(request.RideId),
                cancellationToken);

            return Result.Success();
        }
    }
}