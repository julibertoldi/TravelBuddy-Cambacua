using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace TravelBuddy.Statistics
{
    public interface IStatisticsAppService : IApplicationService
    {
        Task<AdminDashboardDto> GetDashboardStatisticsAsync();
        Task<List<ApiCallLogDto>> GetApiCallLogsAsync();
        Task<IRemoteStreamContent> ExportApiLogsCsvAsync();
        Task<IRemoteStreamContent> ExportSearchLogsToCsvAsync();
    }
}
