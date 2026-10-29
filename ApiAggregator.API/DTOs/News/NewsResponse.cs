using System.Text.Json.Serialization;

namespace ApiAggregator.API.DTOs.News
{
    public class NewsResponse
    {
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("articles")]
        public List<Article> Articles { get; set; } = new();
    }
}