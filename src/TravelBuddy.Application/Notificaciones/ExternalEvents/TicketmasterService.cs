using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelBuddy.Notifications;
using TravelBuddy.Notifications.ExternalEvents;
using Volo.Abp.DependencyInjection;

namespace TravelBuddy.Notificaciones.ExternalEvents
{
    public class TicketmasterService : ITicketmasterService, ITransientDependency
    {
        public Task<List<UpcomingEventNotificationDto>> GetEventsByCityAsync(Guid destinationId, string cityName)
        {
            // Retorna lista vacía ya que la integración real con API externa no se ha implementado en este módulo
            return Task.FromResult(new List<UpcomingEventNotificationDto>());
        }
    }
}
