using System.Net;
using ApiAggregator.API.Clients;
using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.Weather;
using ApiAggregator.API.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiAggregator.API.Tests.Clients
{
    public class ApiClientTests
    {
        // Helper: in-memory configuration, like a tiny appsettings.json for the test
        private static IConfiguration ConfigWithKey(string section, string? apiKey) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [$"{section}:ApiKey"] = apiKey })
                .Build();

        // Helper: a REAL WeatherClient whose network layer is replaced by the fake handler
        private static WeatherClient CreateWeatherClient(FakeHttpMessageHandler handler, string? apiKey = "test-key") =>
            new(new HttpClient(handler) { BaseAddress = new Uri("https://api.openweathermap.org/") },
                ConfigWithKey("OpenWeather", apiKey),
                NullLogger<WeatherClient>.Instance);

        [Fact]
        public async Task ErrorStatusCode_ReturnsFailure_WithStatusInMessage()
        {
            // Arrange: the fake API always answers with 500
            var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);
            var client = CreateWeatherClient(handler);

            // Act
            var result = await client.FetchDataAsync(new AggregateQueryParameters(), CancellationToken.None);

            // Assert: failure, and the status code is mentioned in the message
            Assert.False(result.IsSuccess);
            Assert.Contains("500", result.ErrorMessage);
        }

        [Fact]
        public async Task Weather_SendsApiKeyAndCoordinates_AndParsesResponse()
        {
            // Arrange: the fake API answers with the weather in Athens
            var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "name": "Athens", "cod": 200 }""");
            var client = CreateWeatherClient(handler);

            // Act: request weather for specific coordinates
            var result = await client.FetchDataAsync(new AggregateQueryParameters { Lat = 40.27, Lon = 22.5 }, CancellationToken.None);

            // Assert (response): the client parsed the JSON correctly
            Assert.True(result.IsSuccess);
            var weather = Assert.IsType<WeatherResponse>(result.Data);
            Assert.Equal("Athens", weather.CityName);

            // Assert (request): the client sent the API key and the coordinates
            var request = Assert.Single(handler.Requests);
            var query = request.RequestUri!.Query;
            Assert.Contains("appid=test-key", query);
            Assert.Contains("lat=40.27", query);
            Assert.Contains("lon=22.5", query);
        }

        [Fact]
        public async Task MissingApiKey_ReturnsFailure_WithoutCallingTheApi()
        {
            // Arrange: a client WITHOUT an API key
            var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK);
            var client = CreateWeatherClient(handler, apiKey: null);

            // Act
            var result = await client.FetchDataAsync(new AggregateQueryParameters(), CancellationToken.None);

            // Assert: failure, and no request was ever sent
            Assert.False(result.IsSuccess);
            Assert.Empty(handler.Requests);
        }

        [Fact]
        public async Task NetworkError_ReturnsGenericMessage_AndDoesNotLeakInternalDetails()
        {
            // Arrange: the network call fails with an exception that contains internal details
            var handler = new FakeHttpMessageHandler((_, _) =>
                throw new HttpRequestException("No such host is known (internal-proxy.local:443)"));
            var client = CreateWeatherClient(handler);

            // Act
            var result = await client.FetchDataAsync(new AggregateQueryParameters(), CancellationToken.None);

            // Assert: the user gets a generic message, never the exception details
            Assert.False(result.IsSuccess);
            Assert.Equal("OpenWeather is temporarily unavailable.", result.ErrorMessage);
            Assert.DoesNotContain("internal-proxy", result.ErrorMessage);
        }

        [Fact]
        public async Task Timeout_IsTreatedAsFailure_NotRethrown()
        {
            // Arrange: a timeout throws OperationCanceledException, but OUR token is NOT cancelled
            var handler = new FakeHttpMessageHandler((_, _) =>
                throw new TaskCanceledException("Simulated timeout"));
            var client = CreateWeatherClient(handler);

            // Act
            var result = await client.FetchDataAsync(new AggregateQueryParameters(), CancellationToken.None);

            // Assert: handled as an API failure, not propagated as a cancellation
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UserCancellation_IsRethrown()
        {
            // Arrange: the caller has already cancelled the request
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var handler = new FakeHttpMessageHandler((_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));
            var client = CreateWeatherClient(handler);

            // Act + Assert: the cancellation must bubble up, not be swallowed as a failure
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => client.FetchDataAsync(new AggregateQueryParameters(), cts.Token));
        }
    }
}