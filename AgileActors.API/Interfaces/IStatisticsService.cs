using AgileActors.API.DTOs;

namespace AgileActors.API.Interfaces
{
    public interface IStatisticsService
    {
        void RecordRequest(string apiName, double responseTimeMs);

        IDictionary<string, ApiStatsDto> GetStats();
    }
}