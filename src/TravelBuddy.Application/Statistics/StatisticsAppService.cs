using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Calificaciones;
using TravelBuddy.Permissions;
using TravelBuddy.Statistics;
using Volo.Abp.Content;
using Volo.Abp.AuditLogging;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TravelBuddy.Statistics
{
    [Authorize(TravelBuddyPermissions.Admin.Metrics)]
    public class StatisticsAppService : TravelBuddyAppService, IStatisticsAppService
    {
        private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;
        private readonly IRepository<AuditLog, Guid> _auditLogRepository;
        private readonly IRepository<IdentitySecurityLog, Guid> _securityLogRepository;
        private static readonly string[] FailedLoginActions =
        {
            IdentitySecurityLogActionConsts.LoginFailed,
            IdentitySecurityLogActionConsts.LoginLockedout,
            IdentitySecurityLogActionConsts.LoginNotAllowed,
            IdentitySecurityLogActionConsts.LoginInvalidUserName,
            IdentitySecurityLogActionConsts.LoginInvalidUserNameOrPassword
        };

        private const int AuditEventsLimit = 100;

        private static string Csv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";

            // Evita que Excel interprete el texto como fórmula
            if (value[0] is '=' or '+' or '-' or '@')
                value = "'" + value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public StatisticsAppService(
            IRepository<SearchLogs, Guid> searchLogRepository,
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Favorite> favoriteRepository,
            IRepository<ApiCallLog, Guid> apiCallLogRepository,
            IRepository<AuditLog, Guid> auditLogRepository,
            IRepository<IdentitySecurityLog, Guid> securityLogRepository)
        {
            _searchLogRepository = searchLogRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
            _apiCallLogRepository = apiCallLogRepository;
            _auditLogRepository = auditLogRepository;
            _securityLogRepository = securityLogRepository;
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

            var apiCallsQuery = await _apiCallLogRepository.GetQueryableAsync();

            var totalApiCalls = await AsyncExecuter.CountAsync(apiCallsQuery);

            double avgResponseTime = totalApiCalls > 0
                ? await AsyncExecuter.AverageAsync(apiCallsQuery, x => x.ResponseTimeMs)
                : 0;

            var totalApiErrors = await AsyncExecuter.CountAsync(apiCallsQuery, x => !x.IsSuccess || x.StatusCode >= 400);

            return new AdminDashboardDto
            {
                TotalSearches = totalSearches,
                TotalSavedDestinations = totalSavedDestinations,
                TopDestinations = topDestinations,
                TotalApiCalls = totalApiCalls,
                AverageResponseTimeMs = Math.Round(avgResponseTime, 2),
                TotalApiErrors = totalApiErrors
            };
        }

        public async Task<List<ApiCallLogDto>> GetApiCallLogsAsync()
        {
            var queryableLogs = await _apiCallLogRepository.GetQueryableAsync();

            var logs = await AsyncExecuter.ToListAsync(
                queryableLogs
                    .Where(x => !x.IsSuccess || x.StatusCode >= 400)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(50)
                    .Select(x => new ApiCallLogDto
                    {
                        Id = x.Id,
                        Endpoint = x.Endpoint,
                        StatusCode = x.StatusCode,
                        ResponseTimeMs = x.ResponseTimeMs,
                        IsSuccess = x.IsSuccess,
                        ErrorMessage = x.ErrorMessage,
                        Timestamp = x.Timestamp
                    })
            );

            return logs;
        }

        public async Task<List<AuditLogExceptionDto>> GetInternalErrorLogsAsync()
        {
            var queryableLogs = await _auditLogRepository.GetQueryableAsync();

            return await AsyncExecuter.ToListAsync(
                queryableLogs
                    .Where(x => x.Exceptions != null && x.Exceptions != string.Empty)
                    .OrderByDescending(x => x.ExecutionTime)
                    .Take(50)
                    .Select(x => new AuditLogExceptionDto
                    {
                        ExecutionTime = x.ExecutionTime,
                        HttpMethod = x.HttpMethod,
                        Url = x.Url,
                        HttpStatusCode = x.HttpStatusCode,
                        Exceptions = x.Exceptions
                    }));
        }

        public async Task<List<AuditEventDto>> GetCriticalAuditEventsAsync()
        {
            var favoriteType = typeof(Favorite).FullName!;
            var ratingType = typeof(Calificacion).FullName!;
            var auditLogsQuery = await _auditLogRepository.GetQueryableAsync();
            var entityChangeRowsQuery = auditLogsQuery.SelectMany(
                auditLog => auditLog.EntityChanges
                    .Where(change =>
                        change.EntityTypeFullName == favoriteType ||
                        change.EntityTypeFullName == ratingType)
                    .Select(change => new
                    {
                        change.EntityTypeFullName,
                        change.ChangeType,
                        change.ChangeTime,
                        auditLog.UserName,
                        auditLog.ClientIpAddress,
                        auditLog.Exceptions,
                        auditLog.HttpStatusCode
                    }));
            var entityChangeRows = await AsyncExecuter.ToListAsync(
                entityChangeRowsQuery
                    .OrderByDescending(change => change.ChangeTime)
                    .Take(AuditEventsLimit));

            var userActionsRowsQuery = auditLogsQuery.SelectMany(
                auditLog => auditLog.Actions
                    .Where(action =>
                        action.ServiceName != null &&
                        action.ServiceName.Contains("IdentityUser") &&
                        (action.MethodName == "CreateAsync" ||
                         action.MethodName == "DeleteAsync" ||
                         action.MethodName == "UpdateRolesAsync"))
                    .Select(action => new
                    {
                        action.MethodName,
                        action.ExecutionTime,
                        auditLog.UserName,
                        auditLog.ClientIpAddress,
                        auditLog.Exceptions,
                        auditLog.HttpStatusCode
                    }));
            var userActionRows = await AsyncExecuter.ToListAsync(
                userActionsRowsQuery
                    .OrderByDescending(action => action.ExecutionTime)
                    .Take(AuditEventsLimit));

            var securityLogsQuery = await _securityLogRepository.GetQueryableAsync();
            var failedLoginRows = await AsyncExecuter.ToListAsync(
                securityLogsQuery
                    .Where(log => FailedLoginActions.Contains(log.Action))
                    .OrderByDescending(log => log.CreationTime)
                    .Take(AuditEventsLimit)
                    .Select(log => new
                    {
                        log.UserName,
                        log.Action,
                        log.CreationTime,
                        log.ClientIpAddress
                    }));

            var events = new List<AuditEventDto>();
            events.AddRange(entityChangeRows.Select(row => new AuditEventDto
            {
                UserName = NormalizeUserName(row.UserName),
                Action = DescribeEntityChange(row.EntityTypeFullName, row.ChangeType),
                ExecutionTime = row.ChangeTime,
                ClientIpAddress = row.ClientIpAddress,
                Result = IsFailed(row.Exceptions, row.HttpStatusCode) ? "Fallido" : "Correcto"
            }));
            events.AddRange(userActionRows.Select(row => new AuditEventDto
            {
                UserName = NormalizeUserName(row.UserName),
                Action = DescribeIdentityAction(row.MethodName),
                ExecutionTime = row.ExecutionTime,
                ClientIpAddress = row.ClientIpAddress,
                Result = IsFailed(row.Exceptions, row.HttpStatusCode) ? "Fallido" : "Correcto"
            }));
            events.AddRange(failedLoginRows.Select(row => new AuditEventDto
            {
                UserName = NormalizeUserName(row.UserName),
                Action = row.Action,
                ExecutionTime = row.CreationTime,
                ClientIpAddress = row.ClientIpAddress,
                Result = "Fallido"
            }));

            return events
                .OrderByDescending(auditEvent => auditEvent.ExecutionTime)
                .Take(AuditEventsLimit)
                .ToList();
        }

        private static string NormalizeUserName(string? userName)
        {
            return string.IsNullOrWhiteSpace(userName) ? "Sistema/anónimo" : userName;
        }

        private static string DescribeEntityChange(string entityTypeFullName, EntityChangeType changeType)
        {
            if (entityTypeFullName == typeof(Favorite).FullName)
            {
                return (int)changeType switch
                {
                    0 => "Favorito agregado",
                    1 => "Favorito modificado",
                    2 => "Favorito eliminado",
                    _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Tipo de cambio de entidad desconocido.")
                };
            }

            return (int)changeType switch
            {
                0 => "Calificación agregada",
                1 => "Calificación modificada",
                2 => "Calificación eliminada",
                _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Tipo de cambio de entidad desconocido.")
            };
        }

        private static string DescribeIdentityAction(string methodName)
        {
            return methodName switch
            {
                "CreateAsync" => "Usuario creado",
                "DeleteAsync" => "Usuario eliminado",
                "UpdateRolesAsync" => "Roles de usuario actualizados",
                _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, "Acción de Identity desconocida.")
            };
        }

        private static bool IsFailed(string? exceptions, int? httpStatusCode)
        {
            return !string.IsNullOrWhiteSpace(exceptions) ||
                   (httpStatusCode.HasValue && httpStatusCode.Value >= 400);
        }

        public async Task<IRemoteStreamContent> ExportApiLogsCsvAsync()
        {
            var queryableLogs = await _apiCallLogRepository.GetQueryableAsync();
            var logs = await AsyncExecuter.ToListAsync(
                queryableLogs.OrderByDescending(x => x.Timestamp)
            );
            var dashboard = await GetDashboardStatisticsAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ID,Endpoint,StatusCode,ResponseTimeMs,IsSuccess,ErrorMessage,Timestamp,Metric,Value,TopDestination,ViewCount");
            AppendMetric(sb, "TotalSearches", dashboard.TotalSearches);
            AppendMetric(sb, "TotalSavedDestinations", dashboard.TotalSavedDestinations);
            AppendMetric(sb, "TotalApiCalls", dashboard.TotalApiCalls);
            AppendMetric(sb, "AverageResponseTimeMs", dashboard.AverageResponseTimeMs);
            AppendMetric(sb, "TotalApiErrors", dashboard.TotalApiErrors);

            foreach (var destination in dashboard.TopDestinations)
            {
                sb.AppendLine($",,,,,,,,,{Csv(destination.DestinationName)},{destination.ViewCount}");
            }

            foreach (var log in logs)
            {
                sb.AppendLine(string.Join(",",
                    log.Id,
                    Csv(log.Endpoint),
                    log.StatusCode,
                    log.ResponseTimeMs,
                    log.IsSuccess,
                    Csv(log.ErrorMessage),
                    Csv(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
            }

            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());
            var stream = new MemoryStream(byteArray);

            return new RemoteStreamContent(stream, "api-logs-report.csv", "text/csv");
        }

        private static void AppendMetric(StringBuilder csv, string metricName, object value)
        {
            csv.AppendLine($",,,,,,,{Csv(metricName)},{Csv(Convert.ToString(value, CultureInfo.InvariantCulture))},,");
        }
    }
}