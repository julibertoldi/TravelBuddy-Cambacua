using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Authorization;
using Xunit;

namespace TravelBuddy.Statistics;

public class StatisticsAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IStatisticsAppService _statisticsAppService;
    private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddAlwaysAllowAuthorization();
    }

    public StatisticsAppService_Tests()
    {
        _statisticsAppService = GetRequiredService<IStatisticsAppService>();
        _searchLogRepository = GetRequiredService<IRepository<SearchLogs, Guid>>();
        _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
        _favoriteRepository = GetRequiredService<IRepository<Favorite>>();
        _apiCallLogRepository = GetRequiredService<IRepository<ApiCallLog, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public async Task Should_Calculate_Dashboard_Counts_Average_Errors_And_Top_Destinations()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var before = await _statisticsAppService.GetDashboardStatisticsAsync();
            var destinationIds = new Guid[6];

            for (var index = 0; index < destinationIds.Length; index++)
            {
                var destination = new Destination(
                    Guid.NewGuid(),
                    $"Metric destination {Guid.NewGuid():N}",
                    "Description",
                    "Region",
                    "Country")
                {
                    ViewCount = (index + 1) * 10
                };
                destinationIds[index] = destination.Id;
                await _destinationRepository.InsertAsync(destination, autoSave: true);
            }

            await _searchLogRepository.InsertAsync(
                new SearchLogs(Guid.NewGuid(), "Rome", DateTime.UtcNow),
                autoSave: true);
            await _searchLogRepository.InsertAsync(
                new SearchLogs(Guid.NewGuid(), "Paris", DateTime.UtcNow),
                autoSave: true);
            await _favoriteRepository.InsertAsync(
                new Favorite(Guid.NewGuid(), destinationIds[0]),
                autoSave: true);

            await _apiCallLogRepository.InsertAsync(
                new ApiCallLog(Guid.NewGuid(), "GET /success", 200, 10, true),
                autoSave: true);
            await _apiCallLogRepository.InsertAsync(
                new ApiCallLog(Guid.NewGuid(), "GET /failure", 503, 20, false, "Unavailable"),
                autoSave: true);
            await _apiCallLogRepository.InsertAsync(
                new ApiCallLog(Guid.NewGuid(), "GET /success-2", 200, 30, true),
                autoSave: true);

            var after = await _statisticsAppService.GetDashboardStatisticsAsync();

            after.TotalSearches.ShouldBe(before.TotalSearches + 2);
            after.TotalSavedDestinations.ShouldBe(before.TotalSavedDestinations + 1);
            after.TotalApiCalls.ShouldBe(before.TotalApiCalls + 3);
            after.TotalApiErrors.ShouldBe(before.TotalApiErrors + 1);
            after.AverageResponseTimeMs.ShouldBe(
                Math.Round(((before.AverageResponseTimeMs * before.TotalApiCalls) + 60) /
                           (before.TotalApiCalls + 3), 2));
            after.TopDestinations.Count.ShouldBe(5);
            after.TopDestinations[0].ViewCount.ShouldBe(60);
            after.TopDestinations[4].ViewCount.ShouldBe(20);

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Return_Only_Recent_Failed_Api_Calls()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var endpoint = $"GET /failed-{Guid.NewGuid():N}";
            await _apiCallLogRepository.InsertAsync(
                new ApiCallLog(Guid.NewGuid(), endpoint, 502, 125, false, "Bad gateway"),
                autoSave: true);

            var logs = await _statisticsAppService.GetApiCallLogsAsync();

            logs.ShouldContain(log => log.Endpoint == endpoint && !log.IsSuccess);
            logs.ShouldNotContain(log => log.Endpoint.Contains("/success"));

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Export_Dashboard_Metrics_And_Escape_Api_Log_Fields()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            await _destinationRepository.InsertAsync(
                new Destination(
                    Guid.NewGuid(),
                    "Metric destination",
                    "Description",
                    "Region",
                    "Country")
                {
                    ViewCount = 1
                },
                autoSave: true);
            await _apiCallLogRepository.InsertAsync(
                new ApiCallLog(
                    Guid.NewGuid(),
                    "=SUM(\"x\",1)",
                    500,
                    15,
                    false,
                    "+failed,\"call\""),
                autoSave: true);

            var content = await _statisticsAppService.ExportApiLogsCsvAsync();
            await using var stream = content.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var csv = await reader.ReadToEndAsync();

            csv.ShouldContain("ID,Endpoint,StatusCode,ResponseTimeMs,IsSuccess,ErrorMessage,Timestamp,Metric,Value,TopDestination,ViewCount");
            csv.ShouldContain("\"TotalSearches\"");
            csv.ShouldContain("\"Metric destination");
            csv.ShouldContain("\"'=SUM(\"\"x\"\",1)\"");
            csv.ShouldContain("\"'+failed,\"\"call\"\"\"");

            await uow.CompleteAsync();
        }
    }
}
