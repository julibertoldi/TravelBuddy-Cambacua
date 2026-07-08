using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TravelBuddy.Cities
{
    public class GeoDbCitySearchService : ICitySearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GeoDbCitySearchService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            var baseUrl = _config["GeoDb:BaseUrl"];
            var apiKey = _config["GeoDb:ApiKey"];
            var apiHost = _config["GeoDb:ApiHost"];

            var url = $"{baseUrl}?namePrefix={request.PartialName}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-RapidAPI-Key", apiKey);
            httpRequest.Headers.Add("X-RapidAPI-Host", apiHost);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var result = new CitySearchResultDto();

            foreach (var city in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                result.Cities.Add(new CityDto
                {
                    Id = city.GetProperty("id").GetInt32(),
                    Name = city.GetProperty("city").GetString(),
                    Country = city.GetProperty("country").GetString()
                });
            }

            return result;
        }

        public async Task<CityDetailDto> GetCityDetailsAsync(int cityId)
        {
            var baseUrl = _config["GeoDb:BaseUrl"];
            var apiKey = _config["GeoDb:ApiKey"];
            var apiHost = _config["GeoDb:ApiHost"];

            var url = $"{baseUrl}/{cityId}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-RapidAPI-Key", apiKey);
            httpRequest.Headers.Add("X-RapidAPI-Host", apiHost);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var geoDbResponse = JsonSerializer.Deserialize<GeoDbSingleResponse>(json);

            return new CityDetailDto
            {
                Id = geoDbResponse.Data.Id,
                Name = geoDbResponse.Data.City,
                Country = geoDbResponse.Data.Country,
                Region = geoDbResponse.Data.Region,
                Latitude = geoDbResponse.Data.Latitude,
                Longitude = geoDbResponse.Data.Longitude,
                Population = geoDbResponse.Data.Population
            };
        }

        public async Task<CitySearchResultDto> GetPopularCitiesAsync()
        {
            var baseUrl = _config["GeoDb:BaseUrl"];
            var apiKey = _config["GeoDb:ApiKey"];
            var apiHost = _config["GeoDb:ApiHost"];

            var url = $"{baseUrl}?sort=-population&limit=10";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-RapidAPI-Key", apiKey);
            httpRequest.Headers.Add("X-RapidAPI-Host", apiHost);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var result = new CitySearchResultDto();

            foreach (var city in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                result.Cities.Add(new CityDto
                {
                    Id = city.GetProperty("id").GetInt32(),
                    Name = city.GetProperty("city").GetString(),
                    Country = city.GetProperty("country").GetString()
                });
            }

            return result;
        }
    }
}


