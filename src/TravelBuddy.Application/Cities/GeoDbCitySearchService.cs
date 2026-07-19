using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
            var url = baseUrl;
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.PartialName))
                queryParams.Add($"namePrefix={Uri.EscapeDataString(request.PartialName)}");

            // Filtro combinado de País y Región
            if (!string.IsNullOrWhiteSpace(request.Pais) && !string.IsNullOrWhiteSpace(request.Region))
                url = baseUrl.Replace("/cities", $"/countries/{request.Pais}/regions/{request.Region}/cities");

            // Filtro solo por País
            else if (!string.IsNullOrWhiteSpace(request.Pais))
                queryParams.Add($"countryIds={Uri.EscapeDataString(request.Pais)}");

            // Filtro de Población Mínima
            if (request.PoblacionMinima.HasValue)
                queryParams.Add($"minPopulation={request.PoblacionMinima.Value}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-RapidAPI-Key", _config["GeoDb:ApiKey"]);
            httpRequest.Headers.Add("X-RapidAPI-Host", _config["GeoDb:ApiHost"]);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            return ParseCityList(await response.Content.ReadAsStringAsync());
        }

        private CitySearchResultDto ParseCityList(string json)
        {
            var doc = JsonDocument.Parse(json);
            var result = new CitySearchResultDto();

            foreach (var city in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                result.Cities.Add(new CityDto
                {
                    Id = city.GetProperty("id").GetInt32(),
                    Name = city.GetProperty("city").GetString(),
                    Country = city.GetProperty("country").GetString(),
                    Population = city.TryGetProperty("population", out var pop) ? pop.GetInt32() : 0,
                    Latitude = city.TryGetProperty("latitude", out var lat) ? lat.GetDouble() : 0,
                    Longitude = city.TryGetProperty("longitude", out var lon) ? lon.GetDouble() : 0
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


