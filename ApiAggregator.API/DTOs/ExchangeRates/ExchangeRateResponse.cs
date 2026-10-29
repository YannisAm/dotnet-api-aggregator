using System.Text.Json.Serialization;

namespace ApiAggregator.API.DTOs.ExchangeRates
{
    public class ExchangeRateResponse
    {
        [JsonPropertyName("base_code")]
        public string? BaseCode { get; set; }

        [JsonPropertyName("time_last_update_utc")]
        public string? LastUpdated { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, double> Rates { get; set; } = new Dictionary<string, double>();
    }
}