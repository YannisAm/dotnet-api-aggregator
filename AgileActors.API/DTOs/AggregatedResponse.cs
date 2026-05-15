using AgileActors.API.DTOs.ExchangeRates;
using AgileActors.API.DTOs.News;
using AgileActors.API.DTOs.Weather;

namespace AgileActors.API.DTOs
{
    public class AggregatedResponse
    {
        public ApiResult<WeatherResponse>? Weather { get; set; }
        public ApiResult<NewsResponse>? News { get; set; }
        public ApiResult<ExchangeRateResponse>? ExchangeRates { get; set; }
    }
}