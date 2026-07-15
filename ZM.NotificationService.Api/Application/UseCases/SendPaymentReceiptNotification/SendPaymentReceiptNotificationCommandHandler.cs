using MediatR;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Common.OperationResult;

namespace ZM.NotificationService.Api.Application.UseCases.SendPaymentReceiptNotification
{
    public class SendPaymentReceiptNotificationCommandHandler : IRequestHandler<SendPaymentReceiptNotificationCommand, Result>
    {
        private readonly IEmailSender _emailSender;

        public SendPaymentReceiptNotificationCommandHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(SendPaymentReceiptNotificationCommand request, CancellationToken cancellationToken)
        {
            await _emailSender.SendEmailAsync(
                request.RecipientEmail,
                EmailTemplates.Payment.ReceiptSubject,
                EmailTemplates.Payment.Receipt(request.PaymentId, request.RideId, request.Amount),
                cancellationToken);

            return Result.Success();
        }
    }
}