using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Notificaciones;
using TravelBuddy.Notifications.ExternalEvents;
using TravelBuddy.Users;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace TravelBuddy.Notifications
{
    [AllowAnonymous]
    public class NotificationAppService : ApplicationService, INotificationAppService
    {
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly ITicketmasterService _ticketmasterService;
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;

        public NotificationAppService(
            IRepository<Favorite> favoriteRepository,
            IRepository<Destination, Guid> destinationRepository,
            ITicketmasterService ticketmasterService,
            IRepository<Notification, Guid> notificationRepository,
            IdentityUserManager userManager,
            IEmailSender emailSender)
        {
            _favoriteRepository = favoriteRepository;
            _destinationRepository = destinationRepository;
            _ticketmasterService = ticketmasterService;
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<List<NotificationDto>> GetListAsync()
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue) return new List<NotificationDto>();

            var localNotifications = await _notificationRepository.GetListAsync(n => n.UserId == userId.Value);
            var result = ObjectMapper.Map<List<Notification>, List<NotificationDto>>(localNotifications);

            // Incorporar eventos próximos como notificaciones
            try
            {
                var upcomingEvents = await GetUpcomingFavoriteEventsAsync();
                foreach (var upcoming in upcomingEvents)
                {
                    if (upcoming.FechaEvento >= DateTime.UtcNow.Date)
                    {
                        result.Add(new NotificationDto
                        {
                            Id = Guid.NewGuid(),
                            Title = $"Próximo evento en {upcoming.NombreDestino}",
                            Message = $"El evento '{upcoming.TituloEvento}' será el {upcoming.FechaEvento:d}",
                            CreationTime = DateTime.UtcNow,
                            IsRead = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to fetch upcoming events for notifications.");
            }

            return result.OrderByDescending(n => n.CreationTime).ToList();
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _notificationRepository.FirstOrDefaultAsync(n => n.Id == id);
            if (notification != null && notification.UserId == CurrentUser.Id)
            {
                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task<NotificationSettingsDto> GetSettingsAsync()
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue) return new NotificationSettingsDto();

            var user = await _userManager.GetByIdAsync(userId.Value);
            var preferencesJson = user.GetPreferences();

            if (!string.IsNullOrWhiteSpace(preferencesJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<NotificationSettingsDto>(preferencesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new NotificationSettingsDto();
                }
                catch
                {
                    return new NotificationSettingsDto();
                }
            }

            return new NotificationSettingsDto();
        }

        public async Task UpdateSettingsAsync(NotificationSettingsDto input)
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue) throw new AbpAuthorizationException();

            var user = await _userManager.GetByIdAsync(userId.Value);
            var preferencesJson = JsonSerializer.Serialize(input);
            user.SetPreferences(preferencesJson);
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<UpcomingEventNotificationDto>> GetUpcomingFavoriteEventsAsync()
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue)
            {
                return new List<UpcomingEventNotificationDto>();
            }

            // Obtener los IDs de los destinos favoritos del usuario
            var favoriteDestinationIds = (await _favoriteRepository.GetQueryableAsync())
                .Where(f => f.UsuarioId == userId.Value)
                .Select(f => f.DestinoId)
                .ToList();

            if (!favoriteDestinationIds.Any())
            {
                return new List<UpcomingEventNotificationDto>();
            }

            // Traemos los destinos a memoria mediante GetListAsync() 
            var allDestinations = await _destinationRepository.GetListAsync();

            // Filtra los destinos del usuario y nos quedamos con su Id y Name
            var favoriteDestinations = allDestinations
                .Where(d => favoriteDestinationIds.Contains(d.Id))
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToList();

            var upcomingEvents = new List<UpcomingEventNotificationDto>();

            // las ciudades y llama Ticketmaster
            foreach (var destination in favoriteDestinations)
            {
                var eventsInCity = await _ticketmasterService.GetEventsByCityAsync(destination.Id, destination.Name);
                upcomingEvents.AddRange(eventsInCity);
            }

            return upcomingEvents.OrderBy(e => e.FechaEvento).ToList();

        }
        public async Task CheckDailyFavoritesAsync()
        {
            // Obtener todos los favoritos registrados de la BD
            var favorites = await _favoriteRepository.GetListAsync();

            // Iteractua sobre los destinos marcados como favoritos
            var destinationIds = favorites.Select(f => f.DestinoId).Distinct().ToList();

            foreach (var destId in destinationIds)
            {
                var destination = await _destinationRepository.FirstOrDefaultAsync(d => d.Id == destId);
                if (destination != null)
                {
                    // Consultasi existen nuevos eventos en Ticketmaster para esta ciudad
                    var events = await _ticketmasterService.GetEventsByCityAsync(destination.Id, destination.Name);
                    if (events.Any())
                    {
                        // Si hay eventos, muestra en consla 
                        Logger.LogInformation($"[BackgroundWorker] Se verificaron {events.Count} eventos para {destination.Name}");

                        // Para eviar un meil al usuario 
                        await _emailSender.SendAsync("usuario@email.com", $"Novedades en {destination.Name}", $"Se encontraron {events.Count} eventos nuevos.");





                    }
                }
            }
        }

    }
}