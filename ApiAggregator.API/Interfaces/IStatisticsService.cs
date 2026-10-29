using ApiAggregator.API.DTOs;

namespace ApiAggregator.API.Interfaces
{
    public interface IStatisticsService
    {
        void RecordRequest(string apiName, double responseTimeMs);

        IDictionary<string, ApiStatsDto> GetStats();
    }
}