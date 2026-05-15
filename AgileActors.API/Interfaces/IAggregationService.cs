using AgileActors.API.DTOs;

namespace AgileActors.API.Interfaces
{
    public interface IAggregationService
    {
        Task<AggregatedResponse> GetEverythingAsync(AggregateQueryParameters parameters);
    }
}