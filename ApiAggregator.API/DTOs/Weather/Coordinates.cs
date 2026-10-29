using System.Text.Json.Serialization;

namespace ApiAggregator.API.DTOs.Weather
{
    public class Coordinates
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }
}
