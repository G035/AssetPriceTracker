using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using AssetPriceTracker.Asset.Application.Queries;

namespace AssetPriceTracker.Asset.Infastructure.Web
{
    public class TwelveDataProvider : IFetchAssetPrice
    {
        private readonly ILogger<TwelveDataProvider> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TwelveDataProvider(ILogger<TwelveDataProvider> logger, string apiKey)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<decimal> Handle(string symbol)
        {
            try
            {
                var url = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("status", out JsonElement statusElement))
                {
                    if (statusElement.GetString() == "error")
                    {
                        int code = root.GetProperty("code").GetInt32();
                        string? messsage = root.GetProperty("message").GetString();

                        throw new Exception($"Bad API response. Error code {code}. Message: {messsage}");
                    }
                }

                if (root.TryGetProperty("price", out JsonElement priceElement) &&
                    decimal.TryParse(priceElement.GetString(), out var price))
                {
                    return price;
                }
                throw new InvalidOperationException("Price not found or invalid.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching price for {symbol}: {ex.Message}");
                throw;
            }
        }
    }
}
