using System;
using System.ComponentModel.DataAnnotations;

namespace TravelBuddy.Destinations
{
    public class CreateUpdateDestinationDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public Guid CategoriaId { get; set; }
        public int Population { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}