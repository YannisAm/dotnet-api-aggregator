using ApiAggregator.API.DTOs;

namespace ApiAggregator.API.Interfaces
{
    public interface IApiClientInterface
    {
        Task<object> FetchDataAsync(AggregateQueryParameters parameters);
    }
}