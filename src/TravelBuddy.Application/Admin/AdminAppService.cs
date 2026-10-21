using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Permissions;
using TravelBuddy.Statistics;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TravelBuddy.Admin
{
    [Authorize(TravelBuddyPermissions.Admin.Default)]
    public class AdminAppService : TravelBuddyAppService, IAdminAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;
        private readonly IRepository<Favorite> _favoriteRepository;

        public AdminAppService(
            IIdentityUserRepository userRepository,
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Favorite> favoriteRepository,
            IRepository<ApiCallLog, Guid> apiCallLogRepository)
        {
            _userRepository = userRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
            _apiCallLogRepository = apiCallLogRepository;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _userRepository.GetCountAsync();
            var totalDestinations = await _destinationRepository.GetCountAsync();
            var totalFavorites = await _favoriteRepository.GetCountAsync();
            var apiErrors = await _apiCallLogRepository.CountAsync(
                x => !x.IsSuccess || x.StatusCode >= 400);

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalDestinations = totalDestinations,
                TotalFavorites = totalFavorites,
                ExternalApiErrorsCount = apiErrors
            };
        }
    }
}