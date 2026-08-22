using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Favorites;

public class FavoriteDto : EntityDto
{
    public Guid UsuarioId { get; set; }
    public Guid DestinoId { get; set; }

    // Campos para la vista visual
    public string? Nombre { get; set; }
    public string? Ubicacion { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal Precio { get; set; }
}