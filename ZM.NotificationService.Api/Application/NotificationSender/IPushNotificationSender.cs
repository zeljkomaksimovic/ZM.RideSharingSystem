namespace ZM.NotificationService.Api.Application.NotificationSender
{
    public interface IPushNotificationSender
    {
        Task SendPushAsync(string recipient, string title, string message, CancellationToken cancellationToken = default);
    }
}
