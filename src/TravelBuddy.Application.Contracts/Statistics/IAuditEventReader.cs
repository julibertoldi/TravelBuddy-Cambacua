using System.Collections.Generic;
using System.Threading.Tasks;

namespace TravelBuddy.Statistics;

public interface IAuditEventReader
{
    Task<List<AuditEventDto>> GetCriticalAuditEventsAsync();
}
