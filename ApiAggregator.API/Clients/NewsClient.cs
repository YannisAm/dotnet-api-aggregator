using ApiAggregator.API.DTOs;
using ApiAggregator.API.DTOs.News;
using Microsoft.AspNetCore.WebUtilities;

namespace ApiAggregator.API.Clients
{
    public class NewsClient : ApiClientBase<NewsResponse>
    {
        public NewsClient(HttpClient httpClient, IConfiguration configuration, ILogger<NewsClient> logger)
            : base(httpClient, logger, configuration["News:ApiKey"])
        {
        }

        protected override string SourceName => "News";

        protected override string BuildRequestUrl(AggregateQueryParameters parameters)
        {
            var query = new Dictionary<string, string?>
            {
                ["apiKey"] = ApiKey,
                ["q"] = parameters.NewsKeyword,
                ["from"] = parameters.FromDateFormatted
            };

            if (!string.IsNullOrEmpty(parameters.Language))
                query["language"] = parameters.Language;

            if (!string.IsNullOrEmpty(parameters.SortBy))
                query["sortBy"] = parameters.SortBy;

            if (parameters.Page > 0)
                query["page"] = parameters.Page.ToString();

            if (parameters.PageSize > 0)
                query["pageSize"] = parameters.PageSize.ToString();

            return QueryHelpers.AddQueryString("everything", query);
        }
    }
}