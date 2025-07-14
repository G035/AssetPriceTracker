namespace AssetPriceTracker.Asset.Application.Queries
{
    public interface IFetchAssetPrice
    {
        Task<decimal> Handle(string assetName);
    }
}