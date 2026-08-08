using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelBuddy.Statistics
{
    public class AdminDashboardDto
    {
        public long TotalSearches { get; set; }
        public long TotalSavedDestinations { get; set; }
        public List<DestinationStatDto> TopDestinations { get; set; }
        public long TotalApiCalls { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public long TotalApiErrors { get; set; }
    }

    public class DestinationStatDto
    {
        public string DestinationName { get; set; }
        public int ViewCount { get; set; }
    }
}
