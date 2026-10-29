using ApiAggregator.API.DTOs;
using ApiAggregator.API.Interfaces;
using System.Collections.Concurrent;

namespace ApiAggregator.API.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ConcurrentDictionary<string, ApiStats> _stats = new();

        public void RecordRequest(string apiName, double responseTimeMs)
        {
            var stats = _stats.GetOrAdd(apiName, _ => new ApiStats());

            stats.TotalRequests++;
            stats.TotalResponseTime += responseTimeMs;

            if (responseTimeMs < 100) stats.FastRequests++;
            else if (responseTimeMs <= 200) stats.AverageRequests++;
            else stats.SlowRequests++;
        }

        public IDictionary<string, ApiStatsDto> GetStats()
        {
            return _stats.ToDictionary(
                kvp => kvp.Key,
                kvp => new ApiStatsDto
                {
                    TotalRequests = kvp.Value.TotalRequests,
                    AverageResponseTime = kvp.Value.TotalRequests > 0
                        ? Math.Round(kvp.Value.TotalResponseTime / (double)kvp.Value.TotalRequests, 2)
                        : 0,
                    PerformanceBuckets = new PerformanceBucketsDto
                    {
                        Fast = kvp.Value.FastRequests,
                        Average = kvp.Value.AverageRequests,
                        Slow = kvp.Value.SlowRequests
                    }
                });
        }
    }
}