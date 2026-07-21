using System;

namespace TravelBuddy.Notifications
{
    public class UpcomingEventNotificationDto
    {
        public Guid DestinoId { get; set; }
        public string NombreDestino { get; set; } = string.Empty;
        public string TituloEvento { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string EventUrl { get; set; } = string.Empty;
    }
}