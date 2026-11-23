using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using TravelBuddy.Permissions; 
using TravelBuddy.Statistics;
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
        private readonly MetricsLogWriter _metricsLogWriter;
  

        public DestinationAppService(
            IRepository<Destination, Guid> repository,
            ICitySearchService citySearchService,
            ICurrentUser currentUser,
            MetricsLogWriter metricsLogWriter)
            : base(repository)
        {
            _citySearchService = citySearchService;
            _currentUser = currentUser;
            _metricsLogWriter = metricsLogWriter;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            var searchTerm = request.PartialName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Pais))
            {
                searchTerm = string.IsNullOrWhiteSpace(searchTerm)
                    ? $"País: {request.Pais.Trim()}"
                    : $"{searchTerm} | País: {request.Pais.Trim()}";
            }

            try
            {
                return await _citySearchService.SearchCitiesAsync(request);
            }
            finally
            {
                await _metricsLogWriter.RecordSearchAsync(searchTerm, _currentUser.Id);
            }
        }

        public async Task<CityDetailDto> GetCityDetailsAsync(int cityId)
        {
            var city = await _citySearchService.GetCityDetailsAsync(cityId);
            await _metricsLogWriter.RecordDestinationViewAsync(cityId);
            return city;
        }

        public async Task<CitySearchResultDto> GetPopularCitiesAsync()
        {
            return await _citySearchService.GetPopularCitiesAsync();
        }
        // --- MÉTODOS ADMINISTRATIVOS PROTEGIDOS ---s        
        [Authorize]
        public override async Task<DestinationDto> CreateAsync(CreateUpdateDestinationDto input)
        {
            var destination = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

            await Repository.InsertAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task<DestinationDto> UpdateAsync(Guid id, CreateUpdateDestinationDto input)
        {
            var destination = await Repository.GetAsync(id);

            ObjectMapper.Map(input, destination);

            await Repository.UpdateAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);          
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
        }

        [Authorize]
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
                LastUpdated = DateTime.Now,
                ImageUrl = string.IsNullOrWhiteSpace(city.ImageUrl)
                    ? "https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=400"
                    : city.ImageUrl
            };

            await Repository.InsertAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);
        }
    }
}