using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TravelBuddy.Notifications;
using TravelBuddy.Notifications.ExternalEvents;
using Volo.Abp.DependencyInjection;

namespace TravelBuddy.Notificaciones.ExternalEvents
{
    public class TicketmasterService : ITicketmasterService, ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TicketmasterService> _logger;

        public TicketmasterService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TicketmasterService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<UpcomingEventNotificationDto>> GetEventsByCityAsync(Guid destinationId, string cityName)
        {
            var eventsList = new List<UpcomingEventNotificationDto>();

            var apiKey = _configuration["Ticketmaster:ApiKey"];
            var baseUrl = _configuration["Ticketmaster:BaseUrl"] ?? "https://app.ticketmaster.com/discovery/v2/";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Ticketmaster API Key no configurada.");
                return eventsList;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var requestUrl = $"{baseUrl}events.json?apikey={apiKey}&city={Uri.EscapeDataString(cityName)}&sort=date,asc";

                var response = await client.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al consultar Ticketmaster para {cityName}. Status: {response.StatusCode}");
                    return eventsList;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<TicketmasterResponse>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?._embedded?.Events != null)
                {
                    foreach (var ev in apiResponse._embedded.Events)
                    {
                        eventsList.Add(new UpcomingEventNotificationDto
                        {
                            DestinoId = destinationId,
                            NombreDestino = cityName,
                            TituloEvento = ev.Name ?? "Evento sin título",
                            Categoria = ev.Classifications?[0]?.Segment?.Name ?? "General",
                            FechaEvento = ev.Dates?.Start?.DateTime ?? DateTime.UtcNow,
                            EventUrl = ev.Url ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Excepción al consultar eventos de Ticketmaster para: {cityName}");
            }

            return eventsList;
        }
    }

    #region DTOs de Mapeo Interno JSON de Ticketmaster

    internal class TicketmasterResponse
    {
        [JsonPropertyName("_embedded")]
        public TicketmasterEmbedded? _embedded { get; set; }
    }

    internal class TicketmasterEmbedded
    {
        [JsonPropertyName("events")]
        public List<TicketmasterEvent>? Events { get; set; }
    }

    internal class TicketmasterEvent
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("dates")]
        public TicketmasterDates? Dates { get; set; }

        [JsonPropertyName("classifications")]
        public List<TicketmasterClassification>? Classifications { get; set; }
    }

    internal class TicketmasterDates
    {
        [JsonPropertyName("start")]
        public TicketmasterStart? Start { get; set; }
    }

    internal class TicketmasterStart
    {
        [JsonPropertyName("dateTime")]
        public DateTime? DateTime { get; set; }
    }

    internal class TicketmasterClassification
    {
        [JsonPropertyName("segment")]
        public TicketmasterSegment? Segment { get; set; }
    }

    internal class TicketmasterSegment
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    #endregion
}