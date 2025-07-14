using Microsoft.Extensions.Logging;

using AssetPriceTracker.Asset.Application.Queries;
using AssetPriceTracker.Notification.Application.Commands;

namespace AssetPriceTracker.Asset.Application.Commands;

class TrackAssetCommandHandler
{
    private readonly ILogger<TrackAssetCommandHandler> _logger;
    private readonly IFetchAssetPrice _fetchAssetPrice;
    private readonly NotifyAssetPriceReachedSellPriceCommandHandler _notifyAssetPriceReachedSellPrice;
    private readonly NotifyAssetPriceReachedBuyPriceCommandHandler _notifyAssetPriceReachedBuyPrice;
    public TrackAssetCommandHandler(
        ILogger<TrackAssetCommandHandler> logger,
        IFetchAssetPrice fetchAssetPrice,
        NotifyAssetPriceReachedSellPriceCommandHandler notifyAssetPriceReachedSellPrice,
        NotifyAssetPriceReachedBuyPriceCommandHandler notifyAssetPriceReachedBuyPrice
    )
    {
        _fetchAssetPrice = fetchAssetPrice ?? throw new ArgumentNullException(nameof(fetchAssetPrice));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notifyAssetPriceReachedSellPrice = notifyAssetPriceReachedSellPrice ?? throw new ArgumentNullException(nameof(notifyAssetPriceReachedSellPrice));
        _notifyAssetPriceReachedBuyPrice = notifyAssetPriceReachedBuyPrice ?? throw new ArgumentNullException(nameof(notifyAssetPriceReachedBuyPrice));
    }

    public async Task Handle(string assetName, decimal sellPrice, decimal buyPrice)
    {
        if (sellPrice <= buyPrice)
        {
            throw new InvalidOperationException($"Sell price ({sellPrice}) is lower or equal than buy price ({buyPrice}). Ignored");
        }

        _logger.LogInformation("Tracking the asset quote:");
        _logger.LogInformation($"Asset Name: {assetName}");
        _logger.LogInformation($"Sell price: {sellPrice}");
        _logger.LogInformation($"Buy price: {buyPrice}");

        decimal currentPrice = await _fetchAssetPrice.Handle(assetName);

        _logger.LogInformation($"Current price: {currentPrice}");

        // TODO: Put a rate notification limit
        // TODO: Check if I should notify it on equal
        if (currentPrice <= buyPrice)
        {
            _logger.LogInformation($"Buy scenario: {currentPrice} <= {buyPrice}");
            try
            {
                await _notifyAssetPriceReachedBuyPrice.Handle(assetName, currentPrice, buyPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Buy notification failed (SMTP might be unavailable)");
            }
        }

        if (currentPrice >= sellPrice)
        {
            _logger.LogInformation($"Sell scenario: {currentPrice} >= {sellPrice}");
            try
            {
                await _notifyAssetPriceReachedSellPrice.Handle(assetName, currentPrice, sellPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Sell notification failed (SMTP might be unavailable)");
            }
        }
            }
}
