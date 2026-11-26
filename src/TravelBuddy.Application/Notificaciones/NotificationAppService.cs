using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using TravelBuddy.Notifications.ExternalEvents;
using TravelBuddy.Notificaciones;
using Volo.Abp.Identity;
using TravelBuddy.Users;
using System.Text.Json;
using Volo.Abp;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Emailing;

namespace TravelBuddy.Notifications
{
    [Authorize]
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
            return ObjectMapper.Map<List<Notification>, List<NotificationDto>>(localNotifications)
                .OrderByDescending(n => n.CreationTime)
                .ToList();
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
            var preferencesJson = user.GetNotificationSettings() ?? user.GetPreferences();
            return NotificationSettingsStorage.Deserialize(preferencesJson).ToDto();
        }

        public async Task UpdateSettingsAsync(NotificationSettingsDto input)
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue) throw new AbpAuthorizationException();

            var user = await _userManager.GetByIdAsync(userId.Value);
            var settings = NotificationSettingsStorage.Deserialize(user.GetNotificationSettings() ?? user.GetPreferences());
            settings.UpdateFrom(input);
            user.SetNotificationSettings(JsonSerializer.Serialize(settings));
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<UpcomingEventNotificationDto>> GetUpcomingFavoriteEventsAsync()
        {
            var userId = CurrentUser.Id;
            if (!userId.HasValue)
            {
                return new List<UpcomingEventNotificationDto>();
            }

            if (!(await GetSettingsAsync()).UpcomingEventsEnabled)
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
        public async Task EnviarAlertaPorEmail(string userEmail, string destino)
        {
            var userId = CurrentUser.Id ?? throw new AbpAuthorizationException();
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null || user.Id != userId)
            {
                throw new AbpAuthorizationException();
            }

            if (!(await GetSettingsAsync()).EmailNotificationsEnabled)
            {
                return;
            }

            await _emailSender.SendAsync(
                userEmail,
                $"Novedades en tu destino: {destino}",
                $"Se han detectado nuevos eventos en {destino}. Entra a la plataforma para ver más detalles!"
            );
        }
    }
}