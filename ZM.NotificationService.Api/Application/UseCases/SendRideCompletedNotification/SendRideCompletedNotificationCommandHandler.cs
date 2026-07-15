using MediatR;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendRideCompletedNotification
{
    public class SendRideCompletedNotificationCommandHandler : IRequestHandler<SendRideCompletedNotificationCommand, Result>
    {
        private readonly IEmailSender _emailSender;

        public SendRideCompletedNotificationCommandHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(SendRideCompletedNotificationCommand request, CancellationToken cancellationToken)
        {
            await _emailSender.SendEmailAsync(
                request.RecipientEmail,
                EmailTemplates.Ride.CompletedSubject,
                EmailTemplates.Ride.Completed(request.RideId),
                cancellationToken);

            return Result.Success();
        }
    }
}