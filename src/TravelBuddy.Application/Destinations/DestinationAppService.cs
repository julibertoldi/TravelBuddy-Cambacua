using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using TravelBuddy.Permissions; 
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TravelBuddy.Destinations
{
    public class DestinationAppService :
        CrudAppService<
            Destination,
            DestinationDto,
            Guid,
            Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto,
            CreateUpdateDestinationDto>, IDestinationAppService 
    {
        private readonly ICitySearchService _citySearchService;
        private readonly ICurrentUser _currentUser;
  

        public DestinationAppService(
            IRepository<Destination, Guid> repository,
            ICitySearchService citySearchService,
            ICurrentUser currentUser)
            : base(repository)
        {
            _citySearchService = citySearchService;
            _currentUser = currentUser;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            return await _citySearchService.SearchCitiesAsync(request);
        }

        public async Task<CityDetailDto> GetCityDetailsAsync(int cityId)
        {
            return await _citySearchService.GetCityDetailsAsync(cityId);
        }

        public async Task<CitySearchResultDto> GetPopularCitiesAsync()
        {
            return await _citySearchService.GetPopularCitiesAsync();
        }
        // --- MÉTODOS ADMINISTRATIVOS PROTEGIDOS ---

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task<DestinationDto> CreateAsync(CreateUpdateDestinationDto input)
        {
            return await base.CreateAsync(input);
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task<DestinationDto> UpdateAsync(Guid id, CreateUpdateDestinationDto input)
        {
            return await base.UpdateAsync(id, input);
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public async Task<DestinationDto> ImportFromGeoDbAsync(int geoDbCityId)
        {
            var city = await _citySearchService.GetCityDetailsAsync(geoDbCityId);

            var existing = await Repository.FirstOrDefaultAsync(d => d.GeoDbCityId == geoDbCityId);
            if (existing != null)
                return ObjectMapper.Map<Destination, DestinationDto>(existing);

            var destination = new Destination(GuidGenerator.Create(), city.Name,
                $"Ciudad importada desde GeoDB ({city.Country})", city.Region, city.Country)
            {
                GeoDbCityId = geoDbCityId,
                Population = city.Population,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                LastUpdated = DateTime.Now
            };

            await Repository.InsertAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);
        }
    }
}