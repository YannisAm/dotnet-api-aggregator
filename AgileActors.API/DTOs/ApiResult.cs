using AgileActors.API.Interfaces;

namespace AgileActors.API.DTOs
{
    public class ApiResult<T> : IApiResult
    {
        public string Source { get; set; }
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}