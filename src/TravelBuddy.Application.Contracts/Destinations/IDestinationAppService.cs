using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Destinations;

public interface IDestinationAppService :
    ICrudAppService<
        DestinationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateDestinationDto>
{
    Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request);
    Task<CityDetailDto> GetCityDetailsAsync(int cityId);
    Task<CitySearchResultDto> GetPopularCitiesAsync();
    Task<DestinationDto> ImportFromGeoDbAsync(int geoDbCityId);
}