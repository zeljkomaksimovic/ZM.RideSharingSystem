using ZM.NotificationService.Api.Application.NotificationSender;

namespace ZM.NotificationService.Api.Infrastructure.Sms
{
    public class SmsSender : ISmsSender
    {
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(ILogger<SmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}. Message: {Message}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
