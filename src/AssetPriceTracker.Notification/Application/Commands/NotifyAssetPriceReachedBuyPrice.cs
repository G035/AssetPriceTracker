using System.Collections.Generic;

using AssetPriceTracker.Notification.Application.Services;

namespace AssetPriceTracker.Notification.Application.Commands;

class NotifyAssetPriceReachedBuyPriceCommandHandler
{
    private readonly INotificationService _notificationService;
    private readonly List<string> _recipients;
    public NotifyAssetPriceReachedBuyPriceCommandHandler(
        INotificationService notificationService,
        List<string> recipients
    )
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _recipients = recipients ?? throw new ArgumentNullException(nameof(recipients));
    }

    public async Task Handle(string assetName, decimal currentPrice, decimal buyPrice)
    {
        foreach (string emailAdress in _recipients)
        {
            await _notificationService.Notify(
                emailAdress,
                $"[Asset Price Tracker] - {assetName} at {currentPrice}",
                $"Dear Recipient, \n\nWe're reaching out to inform you that the asset {assetName} is currently priced on {currentPrice}, which exceeds your target buy price of {buyPrice}.\n\nPlease consider taking appropriate action.\n\nBest regards,\nYour Asset Notification Service"
            );
        }
    }
}