using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Notifications
{
    // DTO Notificación General
    public class NotificationDto : EntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreationTime { get; set; }
    }

    // DTO Configuración de Preferencias
    public class NotificationSettingsDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool NewReviewNotificationsEnabled { get; set; }
        public bool UpcomingEventsEnabled { get; set; }
    }

}