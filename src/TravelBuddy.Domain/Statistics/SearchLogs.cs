using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace TravelBuddy.Statistics
{
    public class SearchLogs : AggregateRoot<Guid>
    {
        public string SearchTerm { get; set; }
        public DateTime SearchTime { get; set; }
        public Guid? UserId { get; set; }

        protected SearchLogs() { }

        public SearchLogs(Guid id, string searchTerm, DateTime searchTime, Guid? userId = null) : base(id)
        {
            SearchTerm = searchTerm;
            SearchTime = searchTime;
            UserId = userId;
        }
    }
}