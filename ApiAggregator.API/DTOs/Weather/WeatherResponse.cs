using System.Text.Json.Serialization;

namespace ApiAggregator.API.DTOs.Weather
{
    public class WeatherResponse
    {
        [JsonPropertyName("coord")]
        public Coordinates? Coord { get; set; }

        [JsonPropertyName("weather")]
        public List<WeatherDetail> Weather { get; set; } = new();

        [JsonPropertyName("main")]
        public MainData? Main { get; set; }

        [JsonPropertyName("name")]
        public string? CityName { get; set; }

        [JsonPropertyName("cod")]
        public int StatusCode { get; set; }
    }
}