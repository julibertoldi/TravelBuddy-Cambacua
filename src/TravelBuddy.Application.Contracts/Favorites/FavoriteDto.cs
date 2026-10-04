using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Favorites;

public class FavoriteDto : EntityDto
{      public Guid UsuarioId { get; set; }
    public Guid DestinoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Precio { get; set; }

     public int Population { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreationTime { get; set; }
}