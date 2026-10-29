using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.DTOs.Weather;

namespace ApiAggregator.API.DTOs
{
    public class AggregatedResponse
    {
        public ApiResult<WeatherResponse>? Weather { get; set; }
        public ApiResult<NewsResponse>? News { get; set; }
        public ApiResult<ExchangeRateResponse>? ExchangeRates { get; set; }
    }
}