using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Permissions;
using TravelBuddy.Statistics;
using Volo.Abp.Content;
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
            IRepository<ApiCallLog, Guid> apiCallLogRepository)
        {
            _searchLogRepository = searchLogRepository;
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
            _apiCallLogRepository = apiCallLogRepository;
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

        public async Task<IRemoteStreamContent> ExportApiLogsCsvAsync()
        {
            var queryableLogs = await _apiCallLogRepository.GetQueryableAsync();
            var logs = await AsyncExecuter.ToListAsync(
                queryableLogs.OrderByDescending(x => x.Timestamp)
            );

            var sb = new StringBuilder();
            // Cabecera del CSV
            sb.AppendLine("ID,Endpoint,StatusCode,ResponseTimeMs,IsSuccess,ErrorMessage,Timestamp");

            // Filas de datos
            foreach (var log in logs)
            {
                sb.AppendLine(string.Join(",",
                    log.Id,
                    Csv(log.Endpoint),
                    log.StatusCode,
                    log.ResponseTimeMs,
                    log.IsSuccess,
                    Csv(log.ErrorMessage),
                    log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());
            var stream = new MemoryStream(byteArray);

            return new RemoteStreamContent(stream, "api-logs-report.csv", "text/csv");
        }
    }
}