using ApiAggregator.API.DTOs;

namespace ApiAggregator.API.Interfaces
{
    public interface IApiClientInterface
    {
        Task<IApiResult> FetchDataAsync(AggregateQueryParameters parameters);
    }
}