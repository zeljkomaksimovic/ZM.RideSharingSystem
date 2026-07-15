namespace ZM.NotificationService.Api.Application.NotificationSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
    }
}
