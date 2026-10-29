using System.Net;
using ApiAggregator.API.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAggregator.API.Tests.Integration
{
    /// <summary>
    /// Boots the real application in memory for integration tests. Everything is real except the network layer, which
    /// is replaced by a fake handler.
    /// </summary>
    public class ApiAggregatorFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Fake API keys, so the clients actually send requests (to the fake handler)
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["OpenWeather:ApiKey"] = "test-key",
                    ["News:ApiKey"] = "test-key",
                    ["ExchangeRates:ApiKey"] = "test-key"
                }));

            builder.ConfigureTestServices(services =>
            {
                // Replace the real network layer of every HttpClient with the fake handler.
                // The resilience handler from Program.cs stays in place.
                services.ConfigureHttpClientDefaults(http =>
                    http.ConfigurePrimaryHttpMessageHandler(() =>
                        FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}")));
            });
        }
    }
}