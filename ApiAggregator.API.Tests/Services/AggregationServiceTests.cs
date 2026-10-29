using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.DTOs.Weather;
using ApiAggregator.API.Interfaces;
using ApiAggregator.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace ApiAggregator.API.Tests.Services
{
    public class AggregationServiceTests
    {
        private readonly Mock<IStatisticsService> _statsMock;
        private readonly IMemoryCache _cache;

        public AggregationServiceTests()
        {
            _statsMock = new Mock<IStatisticsService>();
            var options = Options.Create(new MemoryCacheOptions());
            _cache = new MemoryCache(options);
        }

        [Fact]
        public async Task GetEverythingAsync_ShouldCallAllClientsAndReturnData()
        {
            var clientMock = new Mock<IApiClientInterface>();

            clientMock
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<object> { IsSuccess = true, Source = "Test" });

            var clients = new List<IApiClientInterface> { clientMock.Object };

            var service = new AggregationService(clients, _statsMock.Object, _cache);

            var result = await service.GetEverythingAsync(new AggregateQueryParameters(), CancellationToken.None);

            Assert.NotNull(result);
            clientMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFallback()
        {
            var weatherMock = new Mock<IApiClientInterface>();
            var newsMock = new Mock<IApiClientInterface>();
            var exchangeMock = new Mock<IApiClientInterface>();

            weatherMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<WeatherResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "API returned success but empty data.",
                    Source = "OpenWeather"
                });

            newsMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<NewsResponse>
                {
                    IsSuccess = true,
                    Source = "News"
                });

            exchangeMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<ExchangeRateResponse>
                {
                    IsSuccess = true,
                    Source = "ExchangeRates"
                });

            var clients = new List<IApiClientInterface>
            {
                weatherMock.Object,
                newsMock.Object,
                exchangeMock.Object
            };

            var service = new AggregationService(clients, _statsMock.Object, _cache);
            var result = await service.GetEverythingAsync(new AggregateQueryParameters(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.False(result.Results["OpenWeather"].IsSuccess);
            Assert.Contains("API returned success but empty data.", result.Results["OpenWeather"].ErrorMessage);
            Assert.True(result.Results["News"].IsSuccess);
            Assert.True(result.Results["ExchangeRates"].IsSuccess);
            weatherMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
            newsMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
            exchangeMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetEverythingAsync_IncludesNewSource_WithoutAggregatorChanges()
        {
            var stocksMock = new Mock<IApiClientInterface>();
            stocksMock
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<string> { Source = "Stocks", IsSuccess = true, Data = "AAPL 230.10" });

            var service = new AggregationService(new[] { stocksMock.Object }, _statsMock.Object, _cache);

            var result = await service.GetEverythingAsync(new AggregateQueryParameters(), CancellationToken.None);

            Assert.True(result.Results.ContainsKey("Stocks"));
            Assert.Equal("AAPL 230.10", result.Results["Stocks"].Data);
        }

        [Fact]
        public async Task GetEverythingAsync_DoesNotCache_WhenASourceFails()
        {
            var failingClient = new Mock<IApiClientInterface>();
            failingClient
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<string> { Source = "OpenWeather", IsSuccess = false, ErrorMessage = "Timeout" });

            var service = new AggregationService(new[] { failingClient.Object }, _statsMock.Object, _cache);
            var parameters = new AggregateQueryParameters();

            await service.GetEverythingAsync(parameters, CancellationToken.None);
            await service.GetEverythingAsync(parameters, CancellationToken.None);

            // Failure MUST NOT be cached, so the client is called both times
            failingClient.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetEverythingAsync_UsesCache_WhenAllSourcesSucceed()
        {
            var okClient = new Mock<IApiClientInterface>();
            okClient
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<string> { Source = "OpenWeather", IsSuccess = true, Data = "sunny" });

            var service = new AggregationService(new[] { okClient.Object }, _statsMock.Object, _cache);
            var parameters = new AggregateQueryParameters();

            await service.GetEverythingAsync(parameters, CancellationToken.None);
            await service.GetEverythingAsync(parameters, CancellationToken.None);

            // Success MUST be cached, so the client is called only once
            okClient.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetEverythingAsync_PassesCancellationToken_ToClients()
        {
            using var cts = new CancellationTokenSource();
            var client = new Mock<IApiClientInterface>();
            client
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResult<string> { Source = "OpenWeather", IsSuccess = true });

            var service = new AggregationService(new[] { client.Object }, _statsMock.Object, _cache);

            await service.GetEverythingAsync(new AggregateQueryParameters(), cts.Token);

            // Ο client πρέπει να πήρε ΑΚΡΙΒΩΣ το token που δώσαμε, όχι κάποιο άλλο
            client.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetEverythingAsync_Throws_WhenRequestIsCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var client = new Mock<IApiClientInterface>();
            client
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>(), It.IsAny<CancellationToken>()))
                .Returns<AggregateQueryParameters, CancellationToken>((_, ct) => Task.FromCanceled<IApiResult>(ct));

            var service = new AggregationService(new[] { client.Object }, _statsMock.Object, _cache);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.GetEverythingAsync(new AggregateQueryParameters(), cts.Token));
        }
    }
}