using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TravelBuddy.Statistics;

namespace TravelBuddy.Infraestructure
{
    public class GeoDbMetricsHandler : DelegatingHandler
    {
        private readonly ILogger<GeoDbMetricsHandler> _logger;
        private readonly MetricsLogWriter _metricsLogWriter;

        public GeoDbMetricsHandler(
            ILogger<GeoDbMetricsHandler> logger,
            MetricsLogWriter metricsLogWriter)
        {
            _logger = logger;
            _metricsLogWriter = metricsLogWriter;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var endpoint = $"{request.Method} {request.RequestUri?.AbsolutePath}";
            try
            {
                _logger.LogInformation("Iniciando llamada a API externa GeoDB: {Endpoint}", endpoint);

                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();
                var isSuccess = response.IsSuccessStatusCode;
                var errorMessage = isSuccess ? null : response.ReasonPhrase;

                await _metricsLogWriter.RecordApiCallAsync(
                    endpoint,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    isSuccess,
                    errorMessage);

                if (isSuccess)
                {
                    _logger.LogInformation("Llamada a GeoDB exitosa en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("GeoDB respondió con error HTTP {StatusCode} en {ElapsedMilliseconds} ms", response.StatusCode, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await _metricsLogWriter.RecordApiCallAsync(
                    endpoint,
                    0,
                    stopwatch.ElapsedMilliseconds,
                    false,
                    ex.Message);
                _logger.LogError(ex, "Fallo crítico de comunicación con la API externa GeoDB tras {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
