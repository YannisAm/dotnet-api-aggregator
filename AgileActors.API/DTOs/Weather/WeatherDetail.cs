using System.Text.Json.Serialization;

namespace AgileActors.API.DTOs.Weather
{
    public class WeatherDetail
    {
        [JsonPropertyName("main")]
        public string Main { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }
}
