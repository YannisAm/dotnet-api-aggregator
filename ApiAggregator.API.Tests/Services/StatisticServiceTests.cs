using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.DTOs.Weather;
using ApiAggregator.API.Interfaces;
using ApiAggregator.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAggregator.API.Tests.Services
{
    public class StatisticServiceTests
    {
        private readonly Mock<IStatisticsService> _statsMock;
        private readonly IMemoryCache _cache;

        public StatisticServiceTests()
        {
            _statsMock = new Mock<IStatisticsService>();
            var options = Options.Create(new MemoryCacheOptions());
            _cache = new MemoryCache(options);
        }

        [Fact]
        public async Task FillTheBuckets()
        {
            var statsMock = new Mock<IStatisticsService>();
            var weatherMock = new Mock<IApiClientInterface>();
            var newsMock = new Mock<IApiClientInterface>();
            var exchangeMock = new Mock<IApiClientInterface>();

            weatherMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<WeatherResponse> { Source = "OpenWeather", IsSuccess = true });

            newsMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<NewsResponse> { Source = "News", IsSuccess = true });

            exchangeMock.Setup(x => x.FetchDataAsync(It.IsAny<AggregateQueryParameters>()))
                .ReturnsAsync(new ApiResult<ExchangeRateResponse> { Source = "ExchangeRates", IsSuccess = true });

            var clients = new List<IApiClientInterface> { weatherMock.Object, newsMock.Object, exchangeMock.Object };

            var service = new AggregationService(clients, statsMock.Object, _cache);

            await service.GetEverythingAsync(new AggregateQueryParameters());

            statsMock.Verify(x => x.RecordRequest("OpenWeather", It.Is<double>(t => t >= 0.0)), Times.Once);
            statsMock.Verify(x => x.RecordRequest("News", It.Is<double>(t => t >= 0.0)), Times.Once);
            statsMock.Verify(x => x.RecordRequest("ExchangeRates", It.Is<double>(t => t >= 0.0)), Times.Once);
        }

        [Fact]
        public async Task FillTheBucketsTimedCorrectly()
        {
            var stats = new StatisticsService();
            var apiName = "OpenWeather";

            stats.RecordRequest(apiName, 50);   //fast
            stats.RecordRequest(apiName, 99);   //fast
            stats.RecordRequest(apiName, 100);  //average
            stats.RecordRequest(apiName, 199);  //average
            stats.RecordRequest(apiName, 200);  //average
            stats.RecordRequest(apiName, 201);  //slow
            stats.RecordRequest(apiName, 400);  //slow

            var allStats = stats.GetStats();
            dynamic result = allStats[apiName];

            Assert.Equal(7, result.TotalRequests);

            Assert.Equal(2, result.PerformanceBuckets.Fast);
            Assert.Equal(3, result.PerformanceBuckets.Average);
            Assert.Equal(2, result.PerformanceBuckets.Slow);

            Assert.Equal(178.43, (double)result.AverageResponseTime, 2);
        }

        [Fact]
        public void RecordRequest_IsThreadSafe_UnderParallelLoad()
        {
            var stats = new StatisticsService();
            const int calls = 100_000;

            Parallel.For(0, calls, _ => stats.RecordRequest("OpenWeather", 50));

            var result = stats.GetStats()["OpenWeather"];
            Assert.Equal(calls, result.TotalRequests);
            Assert.Equal(calls, result.PerformanceBuckets.Fast);
        }
    }
}