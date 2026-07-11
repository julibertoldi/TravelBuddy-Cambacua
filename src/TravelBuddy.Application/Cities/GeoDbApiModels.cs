using System.Text.Json.Serialization;

namespace TravelBuddy.Cities;
internal sealed class GeoDbSingleResponse
{
    [JsonPropertyName("data")] public GeoDbCityData Data { get; set; } = null!;
}
internal sealed class GeoDbCityData
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
    [JsonPropertyName("region")] public string Region { get; set; } = string.Empty;
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("population")] public int Population { get; set; }
}
