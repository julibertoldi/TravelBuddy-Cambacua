using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        Task<List<NotificationDto>> GetListAsync();

        Task MarkAsReadAsync(Guid id);

        Task<NotificationSettingsDto> GetSettingsAsync();

        Task UpdateSettingsAsync(NotificationSettingsDto input);

        /// <summary>
        /// Consulta eventos cercanos en la API externa para las ciudades guardadas en favoritos.
        /// </summary>
        Task<List<UpcomingEventNotificationDto>> GetUpcomingFavoriteEventsAsync();
        Task CheckDailyFavoritesAsync();
    }
}