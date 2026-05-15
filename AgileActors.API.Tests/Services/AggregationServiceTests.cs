using AgileActors.API.DTOs;
using AgileActors.API.DTOs.ExchangeRates;
using AgileActors.API.DTOs.News;
using AgileActors.API.DTOs.Weather;
using AgileActors.API.Interfaces;
using AgileActors.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace AgileActors.API.Tests.Services
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
            Assert.NotNull(result.Weather);
            Assert.False(result.Weather.IsSuccess);
            Assert.Contains("API returned success but empty data.", result.Weather.ErrorMessage);
            Assert.True(result.News.IsSuccess);
            Assert.True(result.ExchangeRates.IsSuccess);
            weatherMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
            newsMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
            exchangeMock.Verify(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()), Times.Once);
        }
    }
}