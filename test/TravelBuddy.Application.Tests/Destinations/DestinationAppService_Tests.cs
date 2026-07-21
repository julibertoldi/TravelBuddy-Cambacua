using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using TravelBuddy.Cities;
using TravelBuddy.Destinations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace TravelBuddy.Application.Tests.Destinations;

public class DestinationAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IDestinationAppService _destinationAppService;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private Mock<ICitySearchService> _citySearchServiceMock;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        _citySearchServiceMock = new Mock<ICitySearchService>();

        // Configuración por defecto para búsquedas
        _citySearchServiceMock
            .Setup(s => s.SearchCitiesAsync(It.IsAny<CitySearchRequestDto>()))
            .ReturnsAsync(new CitySearchResultDto
            {
                Cities = new List<CityDto> { new CityDto { Name = "Rio de Janeiro", Country = "Brazil" } }
            });

        // Configuración por defecto para importación
        _citySearchServiceMock
            .Setup(s => s.GetCityDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(new CityDetailDto
            {
                Id = 123,
                Name = "Ciudad Simulada API",
                Region = "Entre Ríos",
                Country = "Argentina"
            });

        services.Replace(ServiceDescriptor.Transient<ICitySearchService>(provider => _citySearchServiceMock.Object));
    }

    public DestinationAppService_Tests()
    {
        _destinationAppService = GetRequiredService<IDestinationAppService>();
        _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    // ==========================================
    // 🔍 PRUEBAS DEL PASO 1 (BÚSQUEDA)
    // ==========================================

    [Fact]
    public async Task ShouldReturnCities_WhenServiceReturnsResults()
    {
        // Envolvemos en un UnitOfWork para asegurar que el contexto de ABP no se destruya antes de tiempo
        using (var uow = _unitOfWorkManager.Begin())
        {
            var request = new CitySearchRequestDto { PartialName = "Rio" };
            var result = await _destinationAppService.SearchCitiesAsync(request);

            result.ShouldNotBeNull();
            result.Cities.ShouldNotBeEmpty();
            result.Cities[0].Name.ShouldBe("Rio de Janeiro");

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenNoCitiesFound()
    {
        _citySearchServiceMock
            .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "Xyzcity")))
            .ReturnsAsync(new CitySearchResultDto { Cities = new List<CityDto>() });

        using (var uow = _unitOfWorkManager.Begin())
        {
            var request = new CitySearchRequestDto { PartialName = "Xyzcity" };
            var result = await _destinationAppService.SearchCitiesAsync(request);

            result.ShouldNotBeNull();
            result.Cities.ShouldBeEmpty();

            await uow.CompleteAsync();
        }
    }

    // ==========================================
    // 💾 PRUEBAS DEL PASO 2 (IMPORTACIÓN)
    // ==========================================

    [Fact]
    public async Task Should_Import_New_Destination_From_GeoDb()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            int testGeoDbId = 123;

            var result = await _destinationAppService.ImportFromGeoDbAsync(testGeoDbId);

            result.ShouldNotBeNull();
            result.GeoDbCityId.ShouldBe(testGeoDbId);

            var dbCheck = await _destinationRepository.FirstOrDefaultAsync(d => d.GeoDbCityId == testGeoDbId);
            dbCheck.ShouldNotBeNull();
            dbCheck.Name.ShouldBe("Ciudad Simulada API");

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Not_Duplicate_If_Destination_Already_Exists()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            int existingGeoDbId = 999;

            var preExistingDestination = new Destination(
                Guid.NewGuid(),
                "Ciudad Existente",
                "Descripción de prueba",
                "Entre Ríos",
                "Argentina"
            )
            {
                GeoDbCityId = existingGeoDbId
            };

            await _destinationRepository.InsertAsync(preExistingDestination, autoSave: true);

            var result = await _destinationAppService.ImportFromGeoDbAsync(existingGeoDbId);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(preExistingDestination.Id);

            var count = await _destinationRepository.CountAsync(d => d.GeoDbCityId == existingGeoDbId);
            count.ShouldBe(1);

            await uow.CompleteAsync();
        }
    }
}