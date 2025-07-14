namespace AssetPriceTracker.Notification.Application.Services
{
    public interface INotificationService
    {
        Task Notify(string toEmail, string subject, string message);
    }
}