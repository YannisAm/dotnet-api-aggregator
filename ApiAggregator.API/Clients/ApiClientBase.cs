using ApiAggregator.API.DTOs;
using ApiAggregator.API.Interfaces;

namespace ApiAggregator.API.Clients
{
    /// <summary>
    /// Shared request flow for all external API clients. Derived clients only define their source name, request URL and
    /// optional post-processing.
    /// </summary>
    public abstract class ApiClientBase<TResponse> : IApiClientInterface
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        protected string ApiKey { get; }

        protected abstract string SourceName { get; }

        protected ApiClientBase(HttpClient httpClient, ILogger logger, string? apiKey)
        {
            _httpClient = httpClient;
            _logger = logger;
            ApiKey = apiKey ?? string.Empty;
        }

        public async Task<IApiResult> FetchDataAsync(AggregateQueryParameters parameters, CancellationToken cancellationToken)
        {
            var result = new ApiResult<TResponse> { Source = SourceName };

            if (string.IsNullOrEmpty(ApiKey))
            {
                _logger.LogWarning("API key for {Source} is not configured.", SourceName);
                result.ErrorMessage = "API key is missing. Please check configuration.";
                return result;
            }

            try
            {
                var url = BuildRequestUrl(parameters);
                using var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("{Source} returned status code {StatusCode}.", SourceName, (int)response.StatusCode);
                    result.ErrorMessage = $"{SourceName} returned an error ({(int)response.StatusCode}).";
                    return result;
                }

                var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
                var processed = data is null ? default : ProcessResponse(data);

                if (processed is null)
                {
                    _logger.LogWarning("{Source} returned an empty or invalid response.", SourceName);
                    result.ErrorMessage = $"{SourceName} returned an empty response.";
                    return result;
                }

                result.Data = processed;
                result.IsSuccess = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request to {Source} failed.", SourceName);
                result.ErrorMessage = $"{SourceName} is temporarily unavailable.";
            }

            return result;
        }

        /// <summary>Builds the relative request URL for this source.</summary>
        protected abstract string BuildRequestUrl(AggregateQueryParameters parameters);

        /// <summary>
        /// Optional hook to transform or validate the response. Return null to treat it as invalid.
        /// </summary>
        protected virtual TResponse? ProcessResponse(TResponse data) => data;
    }
}