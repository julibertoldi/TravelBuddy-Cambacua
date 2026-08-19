using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Statistics;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.IO;
using System.Text;
using Volo.Abp.Content;
using Microsoft.Extensions.Logging;

namespace TravelBuddy.Statistics
{
    public class StatisticsAppService : ApplicationService, IStatisticsAppService
    {
        private readonly IRepository<SearchLogs, Guid> _searchLogRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;

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
            long totalSearches = 0;
            try
            {
                totalSearches = await _searchLogRepository.GetCountAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "No se pudo obtener la cantidad total de búsquedas.");
            }

            var topDestinations = new List<DestinationStatDto>();
            try
            {
                var queryableDestinations = await _destinationRepository.GetQueryableAsync();
                topDestinations = await AsyncExecuter.ToListAsync(
                    queryableDestinations
                        .OrderByDescending(d => d.ViewCount)
                        .Take(5)
                        .Select(d => new DestinationStatDto
                        {
                            DestinationName = d.Name,
                            ViewCount = d.ViewCount
                        })
                );
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "No se pudo obtener el top de destinos.");
            }

            long totalSavedDestinations = 0;
            try
            {
                totalSavedDestinations = await _favoriteRepository.GetCountAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "No se pudo obtener la cantidad de favoritos.");
            }

            long totalApiCalls = 0;
            double avgResponseTime = 0;
            long totalApiErrors = 0;

            try
            {
                var apiCallsQuery = await _apiCallLogRepository.GetQueryableAsync();
                totalApiCalls = await AsyncExecuter.CountAsync(apiCallsQuery);
                avgResponseTime = totalApiCalls > 0
                    ? await AsyncExecuter.AverageAsync(apiCallsQuery, x => x.ResponseTimeMs)
                    : 0;
                totalApiErrors = await AsyncExecuter.CountAsync(apiCallsQuery, x => !x.IsSuccess || x.StatusCode >= 400);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "No se pudieron obtener las métricas de ApiCallLogs.");
            }

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
            try
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
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error al consultar ApiCallLogs de la base de datos.");
                return new List<ApiCallLogDto>();
            }
        }

        public async Task<IRemoteStreamContent> ExportApiLogsCsvAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ID,Endpoint,StatusCode,ResponseTimeMs,IsSuccess,ErrorMessage,Timestamp");

            try
            {
                var queryableLogs = await _apiCallLogRepository.GetQueryableAsync();
                var logs = await AsyncExecuter.ToListAsync(
                    queryableLogs.OrderByDescending(x => x.Timestamp)
                );

                foreach (var log in logs)
                {
                    var errorMsg = log.ErrorMessage?.Replace(",", " ") ?? "";
                    sb.AppendLine($"{log.Id},{log.Endpoint},{log.StatusCode},{log.ResponseTimeMs},{log.IsSuccess},{errorMsg},{log.Timestamp:yyyy-MM-dd HH:mm:ss}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error al exportar ApiCallLogs a CSV.");
            }

            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());
            var stream = new MemoryStream(byteArray);
            return new RemoteStreamContent(stream, "api-logs-report.csv", "text/csv");
        }

        public async Task<IRemoteStreamContent> ExportSearchLogsToCsvAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Id,Termino,Fecha,UsuarioId"); // Cabeceras

            try
            {
                var logs = await _searchLogRepository.GetListAsync();
                foreach (var log in logs)
                {
                    var term = (log.SearchTerm ?? "").Replace(",", ";");
                    builder.AppendLine($"{log.Id},{term},{log.SearchTime},{log.UserId}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error al exportar SearchLogs a CSV.");
            }

            var byteArray = Encoding.UTF8.GetBytes(builder.ToString());
            var stream = new MemoryStream(byteArray);
            return new RemoteStreamContent(stream, "Estadisticas.csv", "text/csv");
        }
    }
}