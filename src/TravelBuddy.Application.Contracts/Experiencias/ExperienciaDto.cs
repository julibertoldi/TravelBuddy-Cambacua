using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Experiencias
{
    public class ExperienciaDto : AuditedEntityDto<Guid>
    {
        public Guid DestinoId { get; set; }
        public Guid UsuarioId { get; set; }
        
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public ExperienciaValoracion Valoracion { get; set; }
        public string PalabrasClave { get; set; }
    }
}
