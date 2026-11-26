using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TravelBuddy.Cities;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Notificaciones;
using TravelBuddy.Notifications.ExternalEvents;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TravelBuddy.Notifications
{
    public class NotificationPeriodicProcessor : ITransientDependency
    {
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly ICitySearchService _citySearchService;
        private readonly ITicketmasterService _ticketmasterService;
        private readonly NotificationDeliveryService _deliveryService;
        private readonly ILogger<NotificationPeriodicProcessor> _logger;

        public NotificationPeriodicProcessor(
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Favorite> favoriteRepository,
            ICitySearchService citySearchService,
            ITicketmasterService ticketmasterService,
            NotificationDeliveryService deliveryService,
            ILogger<NotificationPeriodicProcessor> logger)
        {
            _destinationRepository = destinationRepository;
            _favoriteRepository = favoriteRepository;
            _citySearchService = citySearchService;
            _ticketmasterService = ticketmasterService;
            _deliveryService = deliveryService;
            _logger = logger;
        }

        public async Task RunDailyAsync()
        {
            var destinations = await _destinationRepository.GetListAsync();
            var favorites = await _favoriteRepository.GetListAsync();
            var favoritesByDestination = favorites
                .GroupBy(favorite => favorite.DestinoId)
                .ToDictionary(group => group.Key, group => group.Select(favorite => favorite.UsuarioId).Distinct().ToList());

            foreach (var destination in destinations.Where(destination => destination.GeoDbCityId.HasValue))
            {
                try
                {
                    await RefreshDestinationAsync(destination, favoritesByDestination);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "No se pudo actualizar el destino importado {DestinationId}.", destination.Id);
                }
            }

            foreach (var destinationId in favoritesByDestination.Keys)
            {
                var destination = destinations.FirstOrDefault(item => item.Id == destinationId);
                if (destination == null)
                {
                    continue;
                }

                var events = await _ticketmasterService.GetEventsByCityAsync(destination.Id, destination.Name);
                foreach (var userId in favoritesByDestination[destinationId])
                {
                    foreach (var upcomingEvent in events)
                    {
                        var title = $"Evento en {upcomingEvent.NombreDestino}: {upcomingEvent.TituloEvento}";
                        var message = $"{upcomingEvent.FechaEvento:yyyy-MM-dd HH:mm} | {upcomingEvent.Categoria} | {upcomingEvent.EventUrl}";
                        await _deliveryService.EnqueueAsync(userId, title, message, onlyWhenEventsEnabled: true);
                    }
                }
            }

            var today = DateTime.UtcNow.Date;
            foreach (var userId in favorites.Select(favorite => favorite.UsuarioId).Distinct())
            {
                await _deliveryService.DeliverWeeklySummaryAsync(userId, today);
            }
        }

        private async Task RefreshDestinationAsync(
            Destination destination,
            Dictionary<Guid, List<Guid>> favoritesByDestination)
        {
            var city = await _citySearchService.GetCityDetailsAsync(destination.GeoDbCityId!.Value);
            var changedFields = new List<string>();

            AddChangeIfDifferent(changedFields, destination.Name, city.Name, "nombre");
            AddChangeIfDifferent(changedFields, destination.Region, city.Region, "región");
            if (destination.Population != city.Population) changedFields.Add("población");
            if (destination.Country != city.Country) changedFields.Add("país");
            if (destination.Latitude != city.Latitude) changedFields.Add("latitud");
            if (destination.Longitude != city.Longitude) changedFields.Add("longitud");

            destination.Name = city.Name ?? destination.Name;
            destination.Region = city.Region ?? destination.Region;
            destination.Population = city.Population;
            destination.Country = city.Country ?? destination.Country;
            destination.Latitude = city.Latitude;
            destination.Longitude = city.Longitude;
            destination.LastUpdated = DateTime.UtcNow;
            await _destinationRepository.UpdateAsync(destination);

            if (changedFields.Count == 0 || !favoritesByDestination.TryGetValue(destination.Id, out var userIds))
            {
                return;
            }

            var title = $"Actualización en {destination.Name}";
            var message = $"Se actualizaron datos del destino ({string.Join(", ", changedFields)}).";
            foreach (var userId in userIds)
            {
                await _deliveryService.EnqueueAsync(userId, title, message);
            }
        }

        private static void AddChangeIfDifferent(List<string> changedFields, string current, string? updated, string label)
        {
            if (!string.IsNullOrWhiteSpace(updated) && !string.Equals(current, updated, StringComparison.Ordinal))
            {
                changedFields.Add(label);
            }
        }
    }
}