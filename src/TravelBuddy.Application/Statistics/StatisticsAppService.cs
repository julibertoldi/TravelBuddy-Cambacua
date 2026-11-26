using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Permissions;
using TravelBuddy.Statistics;
using Volo.Abp.Content;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;

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
        private readonly IAuditEventReader _auditEventReader;

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
            IAuditEventReader auditEventReader)
        {
            _searchLogRepository = searchLogRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
            _apiCallLogRepository = apiCallLogRepository;
            _auditLogRepository = auditLogRepository;
            _auditEventReader = auditEventReader;
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

            var logs = await AsyncExecuter.ToListAsync(
                queryableLogs
                    .Where(x => x.Exceptions != null && x.Exceptions != string.Empty)
                    .Where(x => x.Url == null ||
                                (!x.Url.StartsWith("/connect/") &&
                                 !x.Url.StartsWith("/Abp/ApplicationConfigurationScript")))
                    .OrderByDescending(x => x.ExecutionTime)
                    .Take(50)
                    .Select(x => new
                    {
                        ExecutionTime = x.ExecutionTime,
                        HttpMethod = x.HttpMethod,
                        Url = x.Url,
                        HttpStatusCode = x.HttpStatusCode,
                        Exceptions = x.Exceptions
                    }));

            return logs.Select(x => new AuditLogExceptionDto
            {
                ExecutionTime = x.ExecutionTime,
                HttpMethod = x.HttpMethod,
                Url = x.Url,
                HttpStatusCode = x.HttpStatusCode,
                Message = ExtractExceptionMessage(x.Exceptions)
            }).ToList();
        }

        public async Task<List<AuditEventDto>> GetCriticalAuditEventsAsync()
        {
            return await _auditEventReader.GetCriticalAuditEventsAsync();
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

        private static string ExtractExceptionMessage(string? exceptionJson)
        {
            if (string.IsNullOrWhiteSpace(exceptionJson))
            {
                return string.Empty;
            }

            try
            {
                using var document = JsonDocument.Parse(exceptionJson);
                return FindExceptionMessage(document.RootElement) ?? "Mensaje no disponible";
            }
            catch (JsonException)
            {
                return "Mensaje no disponible";
            }
        }

        private static string? FindExceptionMessage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, "message", StringComparison.OrdinalIgnoreCase) &&
                        property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    var message = FindExceptionMessage(property.Value);
                    if (message != null)
                    {
                        return message;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var message = FindExceptionMessage(item);
                    if (message != null)
                    {
                        return message;
                    }
                }
            }

            return null;
        }
    }
}