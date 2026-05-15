using AgileActors.API.DTOs;

namespace AgileActors.API.Interfaces
{
    public interface IApiClientInterface
    {
        Task<object> FetchDataAsync(AggregateQueryParameters parameters);
    }
}