using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Cities;
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
           Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto,IDestinationAppService>
    {
        private readonly ICitySearchService _citySearchService;
        private readonly ICurrentUser _currentUser;

        public DestinationAppService(
            IRepository<Destination, Guid> repository,
            ICitySearchService citySearchService)
            : base(repository)
        {
            _citySearchService = citySearchService;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            var user = _currentUser;
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
    }
}
