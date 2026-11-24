using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TravelBuddy.Calificaciones;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using Volo.Abp.AuditLogging;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Authorization;
using Volo.Abp.SecurityLog;
using Xunit;

namespace TravelBuddy.Statistics;

public class StatisticsAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IStatisticsAppService _statisticsAppService;
    private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;
    private readonly IRepository<IdentitySecurityLog, Guid> _securityLogRepository;
    private readonly IGuidGenerator _guidGenerator;
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
        _auditLogRepository = GetRequiredService<IRepository<AuditLog, Guid>>();
        _securityLogRepository = GetRequiredService<IRepository<IdentitySecurityLog, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
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
    public void Should_Select_Favorites_And_Ratings_For_Entity_History()
    {
        var options = GetRequiredService<IOptions<AbpAuditingOptions>>().Value;

        options.EntityHistorySelectors.ShouldContain(
            selector => selector.Predicate(typeof(Favorite)) &&
                        selector.Predicate(typeof(Calificacion)));
    }

    [Fact]
    public async Task Should_List_Critical_Audit_Events_And_All_Failed_Login_Actions_Without_Request_Parameters()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var executionTime = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var auditLogId = Guid.NewGuid();
            var auditLog = CreateAuditLog(
                auditLogId,
                "admin",
                userId,
                executionTime,
                new[]
                {
                    ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "CreateAsync"),
                    ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "DeleteAsync"),
                    ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "UpdateRolesAsync"),
                    ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "GetListAsync")
                },
                new[]
                {
                    CreateEntityChange(typeof(Favorite), EntityChangeType.Created, executionTime),
                    CreateEntityChange(typeof(Favorite), EntityChangeType.Deleted, executionTime.AddSeconds(1)),
                    CreateEntityChange(typeof(Calificacion), EntityChangeType.Updated, executionTime.AddSeconds(2))
                });
            await _auditLogRepository.InsertAsync(auditLog, autoSave: true);

            await _auditLogRepository.InsertAsync(
                CreateAuditLog(
                    Guid.NewGuid(),
                    null!,
                    Guid.NewGuid(),
                    executionTime.AddSeconds(11),
                    new[]
                    {
                        ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "CreateAsync")
                    },
                    Array.Empty<EntityChangeInfo>()),
                autoSave: true);

            var failedLoginActions = new[]
            {
                IdentitySecurityLogActionConsts.LoginFailed,
                IdentitySecurityLogActionConsts.LoginLockedout,
                IdentitySecurityLogActionConsts.LoginNotAllowed,
                IdentitySecurityLogActionConsts.LoginInvalidUserName,
                IdentitySecurityLogActionConsts.LoginInvalidUserNameOrPassword
            };

            for (var index = 0; index < failedLoginActions.Length; index++)
            {
                await _securityLogRepository.InsertAsync(
                    CreateSecurityLog(
                        failedLoginActions[index],
                        $"failed-user-{index}",
                        executionTime.AddSeconds(index + 3)),
                    autoSave: true);
            }

            await _securityLogRepository.InsertAsync(
                CreateSecurityLog(
                    IdentitySecurityLogActionConsts.LoginSucceeded,
                    "successful-user",
                    executionTime.AddSeconds(10)),
                autoSave: true);

            var events = await _statisticsAppService.GetCriticalAuditEventsAsync();

            events.ShouldContain(auditEvent => auditEvent.Action == "Usuario creado" &&
                                               auditEvent.UserName == "admin");
            events.ShouldContain(auditEvent => auditEvent.Action == "Usuario eliminado");
            events.ShouldContain(auditEvent => auditEvent.Action == "Roles de usuario actualizados");
            events.ShouldContain(auditEvent => auditEvent.UserName == "Sistema/anónimo");
            events.ShouldContain(auditEvent => auditEvent.Action == "Favorito agregado");
            events.ShouldContain(auditEvent => auditEvent.Action == "Favorito eliminado");
            events.ShouldContain(auditEvent => auditEvent.Action == "Calificación modificada");

            foreach (var action in failedLoginActions)
            {
                events.ShouldContain(auditEvent => auditEvent.Action == action && auditEvent.Result == "Fallido");
            }

            events.ShouldNotContain(auditEvent => auditEvent.UserName == "successful-user");
            events.ShouldNotContain(auditEvent => auditEvent.Action == "GetListAsync");
            events.ShouldContain(auditEvent => auditEvent.ClientIpAddress == "203.0.113.15");
            events.Select(auditEvent => auditEvent.ExecutionTime)
                .ShouldBe(events.Select(auditEvent => auditEvent.ExecutionTime)
                    .OrderByDescending(time => time)
                    .ToList());
            events.Count.ShouldBeLessThanOrEqualTo(100);

            var serializedEvents = JsonSerializer.Serialize(events);
            serializedEvents.ShouldNotContain("sensitive-password");
            serializedEvents.ShouldNotContain("sensitive-token");

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Merge_The_Latest_100_Events_From_Each_Source_Then_Limit_And_Sort()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var startTime = DateTime.UtcNow.AddDays(-1);
            var auditLogId = Guid.NewGuid();
            var actions = Enumerable
                .Repeat(
                    ("TravelBuddy.Users.TravelBuddyIdentityUserAppService", "CreateAsync"),
                    101)
                .ToArray();
            var changes = Enumerable
                .Range(0, 101)
                .Select(index => CreateEntityChange(
                    typeof(Favorite),
                    EntityChangeType.Created,
                    startTime.AddSeconds(index * 3)))
                .ToArray();

            await _auditLogRepository.InsertAsync(
                CreateAuditLog(
                    auditLogId,
                    "admin",
                    Guid.NewGuid(),
                    startTime,
                    actions,
                    changes),
                autoSave: true);

            for (var index = 0; index < 101; index++)
            {
                await _securityLogRepository.InsertAsync(
                    CreateSecurityLog(
                        IdentitySecurityLogActionConsts.LoginFailed,
                        $"failed-user-{index}",
                        startTime.AddSeconds(index * 3 + 2)),
                    autoSave: true);
            }

            var events = await _statisticsAppService.GetCriticalAuditEventsAsync();

            events.Count.ShouldBe(100);
            events.ShouldContain(auditEvent => auditEvent.Action == "Favorito agregado");
            events.ShouldContain(auditEvent => auditEvent.Action == "Usuario creado");
            events.ShouldContain(auditEvent => auditEvent.Action == IdentitySecurityLogActionConsts.LoginFailed);
            events.Select(auditEvent => auditEvent.ExecutionTime)
                .ShouldBe(events.Select(auditEvent => auditEvent.ExecutionTime)
                    .OrderByDescending(time => time)
                    .ToList());

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

    private AuditLog CreateAuditLog(
        Guid auditLogId,
        string? userName,
        Guid userId,
        DateTime executionTime,
        (string ServiceName, string MethodName)[] actions,
        EntityChangeInfo[] changes)
    {
        var entityChanges = changes
            .Select(change => new EntityChange(_guidGenerator, auditLogId, change))
            .ToList();
        var auditActions = actions
            .Select((action, index) => new AuditLogAction(
                Guid.NewGuid(),
                auditLogId,
                new AuditLogActionInfo
                {
                    ServiceName = action.ServiceName,
                    MethodName = action.MethodName,
                    Parameters = "{\"password\":\"sensitive-password\",\"token\":\"sensitive-token\"}",
                    ExecutionTime = executionTime.AddSeconds(index * 3 + 1),
                    ExecutionDuration = 1
                }))
            .ToList();

        return new AuditLog(
            auditLogId,
            "TravelBuddy",
            null,
            null,
            userId,
            userName,
            executionTime,
            1,
            "203.0.113.15",
            null,
            null,
            null,
            null,
            "POST",
            "/identity",
            200,
            null,
            null,
            null,
            null,
            new ExtraPropertyDictionary(),
            entityChanges,
            auditActions,
            null,
            null);
    }

    private EntityChangeInfo CreateEntityChange(Type entityType, EntityChangeType changeType, DateTime changeTime)
    {
        return new EntityChangeInfo
        {
            ChangeTime = changeTime,
            ChangeType = changeType,
            EntityId = Guid.NewGuid().ToString(),
            EntityTypeFullName = entityType.FullName,
            PropertyChanges = new()
        };
    }

    private IdentitySecurityLog CreateSecurityLog(string action, string userName, DateTime creationTime)
    {
        return new IdentitySecurityLog(
            _guidGenerator,
            new SecurityLogInfo
            {
                ApplicationName = "TravelBuddy",
                Identity = "Identity",
                Action = action,
                UserName = userName,
                ClientIpAddress = "203.0.113.15",
                CreationTime = creationTime
            });
    }
}
