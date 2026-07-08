using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace TravelBuddy.Cities
{
    public interface ICitySearchService
    {
        Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request);
        Task<CityDetailDto> GetCityDetailsAsync(int cityId);
        Task<CitySearchResultDto> GetPopularCitiesAsync();
    }
}
