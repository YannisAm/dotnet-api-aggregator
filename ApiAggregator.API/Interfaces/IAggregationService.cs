using ApiAggregator.API.DTOs;

namespace ApiAggregator.API.Interfaces
{
    public interface IAggregationService
    {
        Task<AggregatedResponse> GetEverythingAsync(AggregateQueryParameters parameters, CancellationToken cancellationToken);
    }
}