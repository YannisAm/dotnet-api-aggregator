using System.Text.Json.Serialization;

namespace AgileActors.API.DTOs.News
{
    public class Source
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
