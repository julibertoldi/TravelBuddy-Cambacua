using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using Xunit;

//Prueba de integracion sobre GeoDbCitySearchService.
//Contiene dos metodos de prueba (exito o ningun resultado).

namespace TravelBuddy.Application.Tests.Integration
{
    public class GeoDbCitySearchService_Integration_Tests
    {
        private readonly ICitySearchService _service;

        public GeoDbCitySearchService_Integration_Tests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var httpClient = new HttpClient();
            _service = new GeoDbCitySearchService(httpClient, config);
        }

        [Fact]
        public async Task ShouldReturnCities_WhenSearchingValidName()
        {
            var request = new CitySearchRequestDto { PartialName = "Buenos" };

            var result = await _service.SearchCitiesAsync(request);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Cities);
            Assert.Contains(result.Cities, c => c.Name.Contains("Buenos"));
        }

        [Fact]
        public async Task ShouldReturnEmpty_WhenSearchingNonexistentName()
        {
            await Task.Delay(3000); // espera 3 segundos para evitar límite (para las request a la api)
            var request = new CitySearchRequestDto { PartialName = "Xyzqwertycity" };

            var result = await _service.SearchCitiesAsync(request);

            Assert.NotNull(result);
            Assert.Empty(result.Cities);
        }
    }
}

