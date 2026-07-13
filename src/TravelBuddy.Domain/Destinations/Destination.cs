using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelBuddy.Destinations
{
    public class Destination : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int? GeoDbCityId { get; set; }

        // Constructor vacío que exige Entity Framework por detrás
        public Destination()
        {
        }
        public Destination(Guid id, string name, string description, string region, string country)
            : base(id)
        {
            Name = name;
            Description = description;
            Region = region;
            Country = country;
            IsAvailable = true; // Por defecto lo creamos disponible
            Price = 0;          
        }
    }
}