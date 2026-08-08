using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Statistics
{
    public interface IStatisticsAppService : IApplicationService
    {
        Task<AdminDashboardDto> GetDashboardStatisticsAsync();
    }
}
