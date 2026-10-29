using ApiAggregator.API.Interfaces;

namespace ApiAggregator.API.DTOs
{
    public class ApiResult<T> : IApiResult
    {
        public string Source { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        object? IApiResult.Data => Data;
    }
}