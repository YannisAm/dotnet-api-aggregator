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
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<object> { IsSuccess = true, Source = "Test" });

            var clients = new List<IApiClientInterface> { clientMock.Object };

            var service = new AggregationService(clients, _statsMock.Object, _cache);

            var result = await service.GetEverythingAsync(new AggregateQueryParameters());

            Assert.NotNull(result);
            clientMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
        }

        [Fact]
        public async Task GetFallback()
        {
            var weatherMock = new Mock<IApiClientInterface>();
            var newsMock = new Mock<IApiClientInterface>();
            var exchangeMock = new Mock<IApiClientInterface>();

            weatherMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<WeatherResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "API returned success but empty data.",
                    Source = "OpenWeather"
                });

            newsMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<NewsResponse>
                {
                    IsSuccess = true,
                    Source = "News"
                });

            exchangeMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
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
            var result = await service.GetEverythingAsync(new AggregateQueryParameters());

            Assert.NotNull(result);
            Assert.False(result.Results["OpenWeather"].IsSuccess);
            Assert.Contains("API returned success but empty data.", result.Results["OpenWeather"].ErrorMessage);
            Assert.True(result.Results["News"].IsSuccess);
            Assert.True(result.Results["ExchangeRates"].IsSuccess);
            weatherMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
            newsMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
            exchangeMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
        }

        [Fact]
        public async Task GetEverythingAsync_IncludesNewSource_WithoutAggregatorChanges()
        {
            var stocksMock = new Mock<IApiClientInterface>();
            stocksMock
                .Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<string> { Source = "Stocks", IsSuccess = true, Data = "AAPL 230.10" });

            var service = new AggregationService(new[] { stocksMock.Object }, _statsMock.Object, _cache);

            var result = await service.GetEverythingAsync(new AggregateQueryParameters());

            Assert.True(result.Results.ContainsKey("Stocks"));
            Assert.Equal("AAPL 230.10", result.Results["Stocks"].Data);
        }
    }
}