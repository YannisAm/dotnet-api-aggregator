using System.Net;
using ApiAggregator.API.Clients;
using ApiAggregator.API.DTOs;
using ApiAggregator.API.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiAggregator.API.Tests.Clients
{
    public class ApiClientTests
    {
        private static IConfiguration ConfigWithKey(string section, string? apiKey) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [$"{section}:ApiKey"] = apiKey })
                .Build();

        private static WeatherClient CreateWeatherClient(FakeHttpMessageHandler handler, string? apiKey = "test-key") =>
            new(new HttpClient(handler) { BaseAddress = new Uri("https://api.openweathermap.org/") },
                ConfigWithKey("OpenWeather", apiKey),
                NullLogger<WeatherClient>.Instance);

        [Fact]
        public async Task ErrorStatusCode_ReturnsFailure_WithStatusInMessage()
        {
            var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);
            var client = CreateWeatherClient(handler);

            var result = await client.FetchDataAsync(new AggregateQueryParameters(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains("500", result.ErrorMessage);
        }
    }
}