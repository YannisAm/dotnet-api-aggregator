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

            lock (stats)
            {
                stats.TotalRequests++;
                stats.TotalResponseTime += responseTimeMs;

                if (responseTimeMs < 100) stats.FastRequests++;
                else if (responseTimeMs <= 200) stats.AverageRequests++;
                else stats.SlowRequests++;
            }
        }

        public IDictionary<string, ApiStatsDto> GetStats()
        {
            return _stats.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var s = kvp.Value;
                    lock (s)
                    {
                        return new ApiStatsDto
                        {
                            TotalRequests = s.TotalRequests,
                            AverageResponseTime = s.TotalRequests > 0
                                ? Math.Round(s.TotalResponseTime / s.TotalRequests, 2)
                                : 0,
                            PerformanceBuckets = new PerformanceBucketsDto
                            {
                                Fast = s.FastRequests,
                                Average = s.AverageRequests,
                                Slow = s.SlowRequests
                            }
                        };
                    }
                });
        }
    }
}