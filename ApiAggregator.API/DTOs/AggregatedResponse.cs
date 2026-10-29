using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.DTOs.Weather;
using ApiAggregator.API.Interfaces;

namespace ApiAggregator.API.DTOs
{
    public class AggregatedResponse
    {
        public IReadOnlyDictionary<string, IApiResult> Results { get; init; }
            = new Dictionary<string, IApiResult>();
    }
}