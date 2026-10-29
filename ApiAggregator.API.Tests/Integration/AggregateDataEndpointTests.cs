using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApiAggregator.API.Tests.Integration
{
    public class AggregateDataEndpointTests
    {
        // Each test creates its OWN factory (its own server instance),
        // so the rate limiter and the cache are not shared between tests.

        [Fact]
        public async Task GetData_ReturnsOk_WithAllSourcesInResponse()
        {
            // Arrange
            using var factory = new ApiAggregatorFactory();
            var client = factory.CreateClient();

            // Act: a real HTTP request through the whole pipeline
            var response = await client.GetAsync("/api/AggregateData");

            // Assert: status code and JSON shape
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var results = json.GetProperty("results");

            Assert.True(results.GetProperty("OpenWeather").GetProperty("isSuccess").GetBoolean());
            Assert.True(results.GetProperty("News").GetProperty("isSuccess").GetBoolean());
            Assert.True(results.GetProperty("ExchangeRates").GetProperty("isSuccess").GetBoolean());
        }

        [Fact]
        public async Task GetData_Returns429_WithRetryAfter_AfterTenRequests()
        {
            // Arrange
            using var factory = new ApiAggregatorFactory();
            var client = factory.CreateClient();

            // Act: use up the limit of 10 requests per minute
            for (var i = 0; i < 10; i++)
            {
                var ok = await client.GetAsync("/api/AggregateData");
                Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
            }

            var limited = await client.GetAsync("/api/AggregateData");

            // Assert: the 11th request is rejected and tells the client when to retry
            Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
            Assert.NotNull(limited.Headers.RetryAfter);
        }

        [Fact]
        public async Task GetStats_IsNotRateLimited()
        {
            // Arrange: one aggregation call so that statistics exist
            using var factory = new ApiAggregatorFactory();
            var client = factory.CreateClient();
            await client.GetAsync("/api/AggregateData");

            // Act + Assert: well above the limit of 10, every call still succeeds
            for (var i = 0; i < 15; i++)
            {
                var response = await client.GetAsync("/api/AggregateData/stats");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}