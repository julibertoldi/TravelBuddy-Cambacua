using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace TravelBuddy.Statistics
{
    public class StatisticsAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
    {
        private readonly IStatisticsAppService _statisticsAppService;

        public StatisticsAppService_Tests()
        {
            _statisticsAppService = GetRequiredService<IStatisticsAppService>();
        }

        [Fact]
        public async Task Should_Get_Dashboard_Statistics()
        {
            var dashboardData = await _statisticsAppService.GetDashboardStatisticsAsync();

            dashboardData.ShouldNotBeNull();
            dashboardData.TopDestinations.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Get_Api_Call_Logs()
        {
            var logs = await _statisticsAppService.GetApiCallLogsAsync();
            logs.ShouldNotBeNull();
        }
    }
}
