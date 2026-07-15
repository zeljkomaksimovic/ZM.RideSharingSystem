namespace ZM.NotificationService.Api.Application.NotificationSender
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    }
}
