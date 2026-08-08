using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelBuddy.Statistics
{
    public class ApiCallLogDto
    {
        public Guid Id { get; set; }
        public string Endpoint { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
