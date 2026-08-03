using System;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TravelBuddy.Admin
{
    public class AdminAppService : TravelBuddyAppService, IAdminAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Favorite> _favoriteRepository;

        public AdminAppService(
            IIdentityUserRepository userRepository,
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Favorite> favoriteRepository)
        {
            _userRepository = userRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _userRepository.GetCountAsync();
            var totalDestinations = await _destinationRepository.GetCountAsync();
            var totalFavorites = await _favoriteRepository.GetCountAsync();

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalDestinations = totalDestinations,
                TotalFavorites = totalFavorites,
                ExternalApiErrorsCount = 0
            };
        }
    }
}