using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Favorites;

public class FavoriteDto
{
    public Guid DestinoId { get; set; }
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
    public DateTime CreationTime { get; set; }
}