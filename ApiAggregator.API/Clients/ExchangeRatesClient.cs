using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.Interfaces;

namespace ApiAggregator.API.Clients
{
    public class ExchangeRatesClient : IApiClientInterface
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ExchangeRatesClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ExchangeRates:ApiKey"] ?? "";
        }

        public async Task<object> FetchDataAsync(AggregateQueryParameters parameters)
        {
            var rValue = new ApiResult<ExchangeRateResponse>
            {
                Source = "ExchangeRates"
            };

            if (string.IsNullOrEmpty(_apiKey))
            {
                rValue.IsSuccess = false;
                rValue.ErrorMessage = "API key is missing. Please check configuration.";
                return rValue;
            }

            try
            {
                var baseCurrency = parameters.BaseCurrency?.ToUpper();
                var url = $"{_apiKey}/latest/{baseCurrency}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var rawData = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();

                    if (rawData?.Rates != null)
                    {
                        var majorCurrencies = new List<string> { "USD", "EUR", "GBP", "JPY", "CNY" };
                        var filteredRates = new Dictionary<string, double>();

                        foreach (var currency in majorCurrencies)
                        {
                            if (rawData.Rates.TryGetValue(currency, out double rate))
                            {
                                filteredRates.Add(currency, rate);
                            }
                        }

                        rawData.Rates = filteredRates
                            .OrderByDescending(x => x.Value)
                            .ToDictionary(x => x.Key, x => x.Value);

                        rValue.Data = rawData;
                        rValue.IsSuccess = true;
                    }
                    else
                    {
                        rValue.IsSuccess = false;
                        rValue.ErrorMessage = "API returned success but rates data was empty.";
                    }
                }
                else
                {
                    rValue.IsSuccess = false;
                    rValue.ErrorMessage = $"API Error: {(int)response.StatusCode} ({response.ReasonPhrase})";
                }
            }
            catch (Exception ex)
            {
                rValue.IsSuccess = false;
                rValue.ErrorMessage = $"Connection Error: {ex.Message}";
            }

            return rValue;
        }
    }
}