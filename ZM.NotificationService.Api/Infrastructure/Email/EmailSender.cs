using ZM.NotificationService.Api.Application.NotificationSender;

namespace ZM.NotificationService.Api.Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        //Note: This is a mock implementation. In a real-world scenario, you would integrate with an email service provider.
        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending email to {Recipient}. Subject: {Subject}. Body: {Body}", recipient, subject, body);
            return Task.CompletedTask;
        }
    }
}
