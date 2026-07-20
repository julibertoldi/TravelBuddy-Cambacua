using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using TravelBuddy.Destinations;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace TravelBuddy.Application.Tests.Destinations
{
    public class DestinationAppService_Tests
    {
        private readonly Mock<ICitySearchService> _citySearchServiceMock;
        private readonly DestinationAppService _appService;

        public DestinationAppService_Tests()
        {
            var repoMock = new Mock<IRepository<Destination, Guid>>();
            _citySearchServiceMock = new Mock<ICitySearchService>();

            _appService = new DestinationAppService(repoMock.Object, _citySearchServiceMock.Object); 
        }

        [Fact]
        public async Task ShouldReturnCities_WhenServiceReturnsResults()
        {
            // Arrange
            var request = new CitySearchRequestDto { PartialName = "Rio" };
            var expected = new CitySearchResultDto
            {
                Cities = new() { new CityDto { Name = "Rio de Janeiro", Country = "Brazil" } }
            };
            
           
            _citySearchServiceMock
                .Setup(s => s.SearchCitiesAsync(request))
                .ReturnsAsync(expected);

            // Act
            var result = await _appService.SearchCitiesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cities);
            Assert.Equal("Rio de Janeiro", result.Cities[0].Name);
        }

        [Fact]
        public async Task ShouldReturnEmpty_WhenNoCitiesFound()
        {
            var request = new CitySearchRequestDto { PartialName = "Xyzcity" };

            
            _citySearchServiceMock
                .Setup(s => s.SearchCitiesAsync(request))
                .ReturnsAsync(new CitySearchResultDto());

            var result = await _appService.SearchCitiesAsync(request);

            Assert.NotNull(result);
            Assert.Empty(result.Cities);
        }

        [Fact]
        public async Task ShouldThrow_WhenServiceThrowsException()
        {
            var request = new CitySearchRequestDto { PartialName = "ErrorCiudad" };

            _citySearchServiceMock
                .Setup(s => s.SearchCitiesAsync(request))
                .ThrowsAsync(new HttpRequestException("Simulated API failure")); 

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _appService.SearchCitiesAsync(request)
            );
        }
    }
}
