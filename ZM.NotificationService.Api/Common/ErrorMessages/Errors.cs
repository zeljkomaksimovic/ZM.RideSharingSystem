namespace ZM.NotificationService.Api.Common.ErrorMessages
{
    public static class Errors
    {
        public static class Notification
        {
            public static Error EmailSendingFailed() => new("NOTIF001", "Failed to send the email.");
            public static Error SmsSendingFailed() => new("NOTIF002", "Failed to send the SMS.");
            public static Error PushNotificationSendingFailed() => new("NOTIF003", "Failed to send the push notification.");
        }
    }
}
