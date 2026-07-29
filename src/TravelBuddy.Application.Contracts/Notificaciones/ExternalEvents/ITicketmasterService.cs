using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TravelBuddy.Notifications.ExternalEvents
{
    public interface ITicketmasterService
    {
        Task<List<UpcomingEventNotificationDto>> GetEventsByCityAsync(Guid destinationId, string cityName);
    }
}
