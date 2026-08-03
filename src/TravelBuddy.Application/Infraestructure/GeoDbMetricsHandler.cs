using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TravelBuddy.Infraestructure
{
    public class GeoDbMetricsHandler : DelegatingHandler
    {
        private readonly ILogger<GeoDbMetricsHandler> _logger;

        public GeoDbMetricsHandler(ILogger<GeoDbMetricsHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Iniciando llamada a API externa GeoDB: {Uri}", request.RequestUri);

                var response = await base.SendAsync(request, cancellationToken);

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
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
                _logger.LogError(ex, "Fallo crítico de comunicación con la API externa GeoDB tras {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
