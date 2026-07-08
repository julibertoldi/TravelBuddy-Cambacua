using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using Xunit;

namespace TravelBuddy.Application.Tests.Cities
{
    public class GeoDbCitySearchService_Tests
    {
        private readonly ICitySearchService _service;

        public GeoDbCitySearchService_Tests()
        {
            // Cargar configuración del appsettings.json de test
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var httpClient = new HttpClient();
            _service = new GeoDbCitySearchService(httpClient, configuration);
        }

        [Fact]
        public async Task ShouldReturnCitiesWhenSearchingByName()
        {
            // Arrange
            var request = new CitySearchRequestDto { PartialName = "Rio" };

            // Act
            var result = await _service.SearchCitiesAsync(request);

            // Assert
            Assert.NotNull(result); //Verifica CitySearchResultDto no sea nulo.
            Assert.NotEmpty(result.Cities); //Verifica que la lista de ciudades dentro del resultado no esté vacía
            Assert.Contains(result.Cities, c => c.Name.Contains("Rio")); //Verifica que al menos una de las ciudades devueltas en la lista contenga la cadena de interes.
        }
    }
}