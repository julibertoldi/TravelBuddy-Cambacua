using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Admin
{
    public interface IAdminAppService : IApplicationService
    {
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    }

    public class AdminDashboardStatsDto
    {
        public long TotalUsers { get; set; }
        public long TotalDestinations { get; set; }
        public long TotalFavorites { get; set; }
        public long ExternalApiErrorsCount { get; set; }
    }
}
