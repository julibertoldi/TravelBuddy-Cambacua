using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Statistics;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TravelBuddy.Statistics
{
    public class StatisticsAppService : ApplicationService, IStatisticsAppService
    {
        private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Favorite> _favoriteRepository;

        public StatisticsAppService(
            IRepository<SearchLogs, Guid> searchLogRepository,
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Favorite> favoriteRepository)
        {
            _searchLogRepository = searchLogRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
        }
        public async Task<AdminDashboardDto> GetDashboardStatisticsAsync()
        {
            var totalSearches = await _searchLogRepository.GetCountAsync();

            var queryableDestinations = await _destinationRepository.GetQueryableAsync();
            var topDestinations = await AsyncExecuter.ToListAsync(
                queryableDestinations
                    .OrderByDescending(d => d.ViewCount)
                    .Take(5)
                    .Select(d => new DestinationStatDto
                    {
                        DestinationName = d.Name,
                        ViewCount = d.ViewCount
                    })
            );

            var totalSavedDestinations = await _favoriteRepository.GetCountAsync();

            return new AdminDashboardDto
            {
                TotalSearches = totalSearches,
                TotalSavedDestinations = totalSavedDestinations,
                TopDestinations = topDestinations
            };
        }
    }
}