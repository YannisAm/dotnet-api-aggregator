using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.ExchangeRates;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.DTOs.Weather;
using ApiAggregator.API.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.API.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IApiClientInterface> _apiClients;
        private readonly IStatisticsService _statisticsService;
        private readonly IMemoryCache _cache;

        public AggregationService(IEnumerable<IApiClientInterface> apiClients, IStatisticsService statisticsService, IMemoryCache cache)
        {
            _apiClients = apiClients;
            _statisticsService = statisticsService;
            _cache = cache;
        }

        public async Task<AggregatedResponse> GetEverythingAsync(AggregateQueryParameters parameters)
        {
            string cacheKey = CreateCacheKey(parameters);

            if (_cache.TryGetValue(cacheKey, out AggregatedResponse cachedResponse))
            {
                return cachedResponse;
            }

            var tasks = _apiClients.Select(async client =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await client.FetchDataAsync(parameters);
                sw.Stop();

                _statisticsService.RecordRequest(result.Source, sw.ElapsedMilliseconds);

                return result;
            });

            var results = await Task.WhenAll(tasks);

            var finalResponse = new AggregatedResponse
            {
                Results = results.ToDictionary(r => r.Source)
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, finalResponse, cacheOptions);

            return finalResponse;
        }

        private string CreateCacheKey(AggregateQueryParameters p)
        {
            return $"agg-cache:" +
                   $"q={p.NewsKeyword?.ToLower()}|" +
                   $"cat={p.Category?.ToLower()}|" +
                   $"lang={p.Language?.ToLower()}|" +
                   $"page={p.Page}|" +
                   $"size={p.PageSize}|" +
                   $"lat={p.LatString}|" +
                   $"lon={p.LonString}|" +
                   $"units={p.Units?.ToLower()}|" +
                   $"base={p.BaseCurrency?.ToUpper()}|" +
                   $"sort={p.SortBy?.ToLower()}|" +
                   $"order={p.SortOrder?.ToLower()}|" +
                   $"from={p.FromDateFormatted}";
        }
    }
}