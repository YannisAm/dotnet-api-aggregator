using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.ExchangeRates;

namespace ApiAggregator.API.Clients
{
    public class ExchangeRatesClient : ApiClientBase<ExchangeRateResponse>
    {
        private static readonly string[] MajorCurrencies = { "USD", "EUR", "GBP", "JPY", "CNY" };

        public ExchangeRatesClient(HttpClient httpClient, IConfiguration configuration, ILogger<ExchangeRatesClient> logger)
            : base(httpClient, logger, configuration["ExchangeRates:ApiKey"])
        {
        }

        protected override string SourceName => "ExchangeRates";

        protected override string BuildRequestUrl(AggregateQueryParameters parameters)
            => $"{ApiKey}/latest/{parameters.BaseCurrency?.ToUpper()}";

        protected override ExchangeRateResponse? ProcessResponse(ExchangeRateResponse data)
        {
            if (data.Rates is null)
                return null;

            data.Rates = data.Rates
                .Where(rate => MajorCurrencies.Contains(rate.Key))
                .OrderByDescending(rate => rate.Value)
                .ToDictionary(rate => rate.Key, rate => rate.Value);

            return data;
        }
    }
}