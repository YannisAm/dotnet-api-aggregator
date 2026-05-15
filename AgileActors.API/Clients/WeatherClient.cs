using AgileActors.API.DTOs;
using AgileActors.API.DTOs.Weather;
using AgileActors.API.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace AgileActors.API.Clients
{
    public class WeatherClient : IApiClientInterface
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeather:ApiKey"] ?? "";
        }

        public async Task<object> FetchDataAsync(AggregateQueryParameters parameters)
        {
            var rValue = new ApiResult<WeatherResponse>
            {
                Source = "OpenWeather"
            };

            if (string.IsNullOrEmpty(_apiKey))
            {
                rValue.IsSuccess = false;
                rValue.ErrorMessage = "API key is missing. Please check configuration.";
                return rValue;
            }

            try
            {
                var baseUrl = "data/2.5/weather";

                var queryParams = new Dictionary<string, string?>();

                queryParams.Add("appid", _apiKey);
                queryParams.Add("lat", parameters.LatString);
                queryParams.Add("lon", parameters.LonString);

                if (!string.IsNullOrEmpty(parameters.Units))
                {
                    queryParams.Add("units", parameters.Units);
                }

                var finalUrl = QueryHelpers.AddQueryString(baseUrl, queryParams);

                var response = await _httpClient.GetAsync(finalUrl);

                if (response.IsSuccessStatusCode)
                {
                    rValue.Data = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                    rValue.IsSuccess = rValue.Data != null;

                    if (!rValue.IsSuccess)
                        rValue.ErrorMessage = "API returned success but empty data.";
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