using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Destinations
{
    public class DestinationDto : AuditedEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public Guid CategoriaId { get; set; }
        public string CategoriaName { get; set; }
        public int? GeoDbCityId { get; set; }
        public int Population { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}