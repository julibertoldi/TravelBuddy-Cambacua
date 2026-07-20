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
    public int Population { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public Destination(Guid id, string name, string description, string region, string country)
        : base(id)
    {
        Name = name;
        Description = description;
        Region = region;
        Country = country;
        IsAvailable = true;
    }

    public Destination(Guid id, string name, string country, int population, double lat, double lon)
        : base(id)
    {
        Name = name;
        Country = country;
        Population = population;
        Latitude = lat;
        Longitude = lon;
        LastUpdated = DateTime.Now;
        IsAvailable = true;
    }
}
}