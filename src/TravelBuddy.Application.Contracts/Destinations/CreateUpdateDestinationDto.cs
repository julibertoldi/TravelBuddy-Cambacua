using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelBuddy.Destinations
{
    public class CreateUpdateDestinationDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
