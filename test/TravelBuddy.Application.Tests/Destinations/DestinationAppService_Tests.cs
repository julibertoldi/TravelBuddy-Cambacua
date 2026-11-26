using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using Volo.Abp.Authorization;
using TravelBuddy.Cities;
using TravelBuddy.Destinations;
using TravelBuddy.Statistics;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace TravelBuddy.Application.Tests.Destinations;

public class DestinationAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IDestinationAppService _destinationAppService;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
    private FakeCitySearchService _citySearchServiceFake = null!;
    private FakeCityImageProvider _cityImageProviderFake = null!;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddAlwaysAllowAuthorization();

        _citySearchServiceFake = new FakeCitySearchService();
        _cityImageProviderFake = new FakeCityImageProvider();
        services.Replace(ServiceDescriptor.Singleton<ICitySearchService>(_citySearchServiceFake));
        services.Replace(ServiceDescriptor.Singleton<ICityImageProvider>(_cityImageProviderFake));
    }

    public DestinationAppService_Tests()
    {
        _destinationAppService = GetRequiredService<IDestinationAppService>();
        _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
        _searchLogRepository = GetRequiredService<IRepository<SearchLogs, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public async Task ShouldReturnCities_WhenServiceReturnsResults()
    {
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
    public async Task Should_Log_Search_Term_And_Country()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var request = new CitySearchRequestDto
            {
                PartialName = "Rio",
                Pais = "Brazil"
            };

            var result = await _destinationAppService.SearchCitiesAsync(request);
            var log = await _searchLogRepository.FirstOrDefaultAsync(
                item => item.SearchTerm == "Rio | País: Brazil");

            result.Cities.ShouldNotBeEmpty();
            log.ShouldNotBeNull();
            log.SearchTime.ShouldBeGreaterThan(DateTime.MinValue);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenNoCitiesFound()
    {
        _citySearchServiceFake.SearchResult = new CitySearchResultDto
        {
            Cities = new List<CityDto>()
        };

        using (var uow = _unitOfWorkManager.Begin())
        {
            var request = new CitySearchRequestDto { PartialName = "Xyzcity" };
            var result = await _destinationAppService.SearchCitiesAsync(request);

            result.ShouldNotBeNull();
            result.Cities.ShouldBeEmpty();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Import_New_Destination_From_GeoDb()
    {
        _cityImageProviderFake.ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c1/Test.jpg";

        using (var uow = _unitOfWorkManager.Begin())
        {
            int testGeoDbId = 123;

            var result = await _destinationAppService.ImportFromGeoDbAsync(testGeoDbId);

            result.ShouldNotBeNull();
            result.GeoDbCityId.ShouldBe(testGeoDbId);

            var dbCheck = await _destinationRepository.FirstOrDefaultAsync(d => d.GeoDbCityId == testGeoDbId);
            dbCheck.ShouldNotBeNull();
            dbCheck.Name.ShouldBe("Ciudad Simulada API");
            dbCheck.ImageUrl.ShouldBe(_cityImageProviderFake.ImageUrl);
            _cityImageProviderFake.CallCount.ShouldBe(1);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Reuse_Cached_Image_Across_City_Searches()
    {
        const int geoDbCityId = 72001;
        const string imageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c1/Cached.jpg";
        _citySearchServiceFake.SearchResult = SearchResultFor(geoDbCityId);
        _cityImageProviderFake.ImageUrl = imageUrl;

        using (var uow = _unitOfWorkManager.Begin())
        {
            var firstSearch = await _destinationAppService.SearchCitiesAsync(
                new CitySearchRequestDto { PartialName = "City" });
            var secondSearch = await _destinationAppService.SearchCitiesAsync(
                new CitySearchRequestDto { PartialName = "City" });

            firstSearch.Cities[0].ImageUrl.ShouldBe(imageUrl);
            secondSearch.Cities[0].ImageUrl.ShouldBe(imageUrl);
            _cityImageProviderFake.CallCount.ShouldBe(1);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Keep_Searching_Without_Image_When_Image_Provider_Fails()
    {
        const int geoDbCityId = 72002;
        _citySearchServiceFake.SearchResult = SearchResultFor(geoDbCityId);
        _cityImageProviderFake.ErrorToThrow = new HttpRequestException("Wikipedia unavailable");

        using (var uow = _unitOfWorkManager.Begin())
        {
            var firstSearch = await _destinationAppService.SearchCitiesAsync(
                new CitySearchRequestDto { PartialName = "City" });
            var secondSearch = await _destinationAppService.SearchCitiesAsync(
                new CitySearchRequestDto { PartialName = "City" });

            firstSearch.Cities.ShouldHaveSingleItem().ImageUrl.ShouldBeNull();
            secondSearch.Cities.ShouldHaveSingleItem().ImageUrl.ShouldBeNull();
            _cityImageProviderFake.CallCount.ShouldBe(1);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Use_Saved_Destination_Image_Without_Calling_Provider()
    {
        const int geoDbCityId = 72003;
        const string imageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c1/Saved.jpg";
        _citySearchServiceFake.SearchResult = SearchResultFor(geoDbCityId);

        using (var uow = _unitOfWorkManager.Begin())
        {
            await _destinationRepository.InsertAsync(
                new Destination(Guid.NewGuid(), "City", "Description", "Region", "Country")
                {
                    GeoDbCityId = geoDbCityId,
                    ImageUrl = imageUrl
                },
                autoSave: true);

            var result = await _destinationAppService.SearchCitiesAsync(
                new CitySearchRequestDto { PartialName = "City" });

            result.Cities[0].ImageUrl.ShouldBe(imageUrl);
            _cityImageProviderFake.CallCount.ShouldBe(0);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Use_Generic_Image_When_Image_Provider_Fails()
    {
        _cityImageProviderFake.ErrorToThrow = new HttpRequestException("Wikipedia unavailable");

        using (var uow = _unitOfWorkManager.Begin())
        {
            var result = await _destinationAppService.ImportFromGeoDbAsync(124);

            result.ImagenUrl.ShouldBe("/assets/images/generic-5.jpg");
            var saved = await _destinationRepository.FirstOrDefaultAsync(d => d.GeoDbCityId == 124);
            saved!.ImageUrl.ShouldBe(result.ImagenUrl);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Use_Generic_Image_When_Provider_Has_No_Matching_WikiData_Page()
    {
        _cityImageProviderFake.ImageUrl = null;

        using (var uow = _unitOfWorkManager.Begin())
        {
            var result = await _destinationAppService.ImportFromGeoDbAsync(126);

            result.ImagenUrl.ShouldBe("/assets/images/generic-1.jpg");
            _cityImageProviderFake.RequestedWikiDataId.ShouldBe("Q126");
            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Use_Generic_Image_When_Provider_Returns_Untrusted_Url()
    {
        _cityImageProviderFake.ImageUrl = "https://example.test/city.jpg";

        using (var uow = _unitOfWorkManager.Begin())
        {
            var result = await _destinationAppService.ImportFromGeoDbAsync(125);

            result.ImagenUrl.ShouldBe("/assets/images/generic-6.jpg");
            await uow.CompleteAsync();
        }
    }

    private static CitySearchResultDto SearchResultFor(int geoDbCityId)
    {
        return new CitySearchResultDto
        {
            Cities = new List<CityDto>
            {
                new() { Id = geoDbCityId, WikiDataId = $"Q{geoDbCityId}", Name = "City", Country = "Country" }
            }
        };
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
            _cityImageProviderFake.CallCount.ShouldBe(0);
            _citySearchServiceFake.GetCityDetailsCallCount.ShouldBe(0);

            var count = await _destinationRepository.CountAsync(d => d.GeoDbCityId == existingGeoDbId);
            count.ShouldBe(1);

            await uow.CompleteAsync();
        }
    }

    private sealed class FakeCitySearchService : ICitySearchService
    {
        public CitySearchResultDto SearchResult { get; set; } = new()
        {
            Cities = new List<CityDto>
            {
                new() { Name = "Rio de Janeiro", Country = "Brazil" }
            }
        };

        public int GetCityDetailsCallCount { get; private set; }

        public Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            return Task.FromResult(SearchResult);
        }

        public Task<CityDetailDto> GetCityDetailsAsync(int cityId)
        {
            GetCityDetailsCallCount++;
            return Task.FromResult(new CityDetailDto
            {
                Id = cityId,
                WikiDataId = $"Q{cityId}",
                Name = "Ciudad Simulada API",
                Region = "Entre Ríos",
                Country = "Argentina"
            });
        }

        public Task<CitySearchResultDto> GetPopularCitiesAsync()
        {
            return Task.FromResult(new CitySearchResultDto());
        }
    }

    private sealed class FakeCityImageProvider : ICityImageProvider
    {
        public string? ImageUrl { get; set; }
        public Exception? ErrorToThrow { get; set; }
        public int CallCount { get; private set; }
        public string? RequestedWikiDataId { get; private set; }

        public Task<string?> GetImageUrlAsync(
            string cityName,
            string? wikiDataId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            RequestedWikiDataId = wikiDataId;
            if (ErrorToThrow is not null)
                throw ErrorToThrow;

            return Task.FromResult(ImageUrl);
        }
    }

    [Fact]
    public async Task Should_Increment_ViewCount_When_Opening_Saved_Destination_Details()
    {
        var destination = new Destination(
            Guid.NewGuid(),
            "Ciudad consultada",
            "Descripción",
            "Región",
            "País")
        {
            GeoDbCityId = 123,
            ViewCount = 4
        };
        using (var seedUow = _unitOfWorkManager.Begin())
        {
            await _destinationRepository.InsertAsync(destination, autoSave: true);
            await seedUow.CompleteAsync();
        }

        using (var uow = _unitOfWorkManager.Begin())
        {
            await _destinationAppService.GetCityDetailsAsync(123);

            var updated = await _destinationRepository.GetAsync(destination.Id);
            updated.ViewCount.ShouldBe(5);

            await uow.CompleteAsync();
        }
    }
}