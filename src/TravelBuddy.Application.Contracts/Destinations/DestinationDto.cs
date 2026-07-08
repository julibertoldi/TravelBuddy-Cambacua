using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelBuddy.Destinations
{
    public class DestinationDto : Volo.Abp.Application.Dtos.AuditedEntityDto<Guid>
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
    }       
}
