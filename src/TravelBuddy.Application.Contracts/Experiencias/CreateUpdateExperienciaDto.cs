using System;
using System.ComponentModel.DataAnnotations;

namespace TravelBuddy.Experiencias
{
    public class CreateUpdateExperienciaDto
    {
        [Required]
        public Guid DestinoId { get; set; }
        
        [Required]
        [StringLength(128)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(1024)]
        public string Descripcion { get; set; }

        [Required]
        public ExperienciaValoracion Valoracion { get; set; }

        public string PalabrasClave { get; set; }
    }
}
