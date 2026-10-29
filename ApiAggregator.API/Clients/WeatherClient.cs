using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.Weather;
using Microsoft.AspNetCore.WebUtilities;

namespace ApiAggregator.API.Clients
{
    public class WeatherClient : ApiClientBase<WeatherResponse>
    {
        public WeatherClient(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherClient> logger)
            : base(httpClient, logger, configuration["OpenWeather:ApiKey"])
        {
        }

        protected override string SourceName => "OpenWeather";

        protected override string BuildRequestUrl(AggregateQueryParameters parameters)
        {
            var query = new Dictionary<string, string?>
            {
                ["appid"] = ApiKey,
                ["lat"] = parameters.LatString,
                ["lon"] = parameters.LonString
            };

            if (!string.IsNullOrEmpty(parameters.Units))
                query["units"] = parameters.Units;

            return QueryHelpers.AddQueryString("data/2.5/weather", query);
        }
    }
}