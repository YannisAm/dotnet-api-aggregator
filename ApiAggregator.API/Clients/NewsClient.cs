using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.News;
using ApiAggregator.API.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace ApiAggregator.API.Clients
{
    public class NewsClient : IApiClientInterface
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["News:ApiKey"] ?? "";
        }

        public async Task<IApiResult> FetchDataAsync(AggregateQueryParameters parameters)
        {
            var rValue = new ApiResult<NewsResponse>
            {
                Source = "News"
            };

            if (string.IsNullOrEmpty(_apiKey))
            {
                rValue.IsSuccess = false;
                rValue.ErrorMessage = "API key is missing. Please check configuration.";
                return rValue;
            }

            try
            {
                var baseUrl = "everything";

                var queryParams = new Dictionary<string, string?>();

                queryParams.Add("apiKey", _apiKey);

                queryParams.Add("q", parameters.NewsKeyword);

                if (!string.IsNullOrEmpty(parameters.FromDateFormatted))
                    queryParams.Add("from", parameters.FromDateFormatted);

                if (!string.IsNullOrEmpty(parameters.Language))
                    queryParams.Add("language", parameters.Language);

                if (!string.IsNullOrEmpty(parameters.SortBy))
                    queryParams.Add("sortBy", parameters.SortBy);

                if (parameters.Page > 0)
                    queryParams.Add("page", parameters.Page.ToString());

                if (parameters.PageSize > 0)
                    queryParams.Add("pageSize", parameters.PageSize.ToString());

                var finalUrl = QueryHelpers.AddQueryString(baseUrl, queryParams);

                var response = await _httpClient.GetAsync(finalUrl);

                if (response.IsSuccessStatusCode)
                {
                    rValue.Data = await response.Content.ReadFromJsonAsync<NewsResponse>();
                    rValue.IsSuccess = rValue.Data != null;

                    if (!rValue.IsSuccess)
                        rValue.ErrorMessage = "API returned 200 but the body was empty.";
                }
                else
                {
                    rValue.IsSuccess = false;
                    rValue.ErrorMessage = $"API Error: {(int)response.StatusCode} ({response.ReasonPhrase})";
                }
            }
            catch (Exception ex)
            {
                rValue.IsSuccess = false;
                rValue.ErrorMessage = $"Connection Error: {ex.Message}";
            }

            return rValue;
        }
    }
}