using ZM.NotificationService.Api.Application.NotificationSender;

namespace ZM.NotificationService.Api.Infrastructure.Push
{
    public class PushNotificationSender : IPushNotificationSender
    {
        private readonly ILogger<PushNotificationSender> _logger;

        public PushNotificationSender(ILogger<PushNotificationSender> logger)
        {
            _logger = logger;
        }

        public Task SendPushAsync(string recipient, string title, string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending push notification to {Recipient}. Title: {Title}. Message: {Message}", recipient, title, message);
            return Task.CompletedTask;
        }
    }
}
