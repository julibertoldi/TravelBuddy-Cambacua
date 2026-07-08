using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelBuddy.Experiencias
{
    public class Experiencia : AuditedAggregateRoot<Guid>
    {
        public Guid DestinoId { get; set; }
        public Guid UsuarioId { get; set; }
        
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public ExperienciaValoracion Valoracion { get; set; }
        
        // E.g., "Gastronomía, Seguridad, Transporte"
        public string PalabrasClave { get; set; }

        protected Experiencia()
        {
        }

        public Experiencia(Guid id, Guid destinoId, Guid usuarioId, string titulo, string descripcion, ExperienciaValoracion valoracion, string palabrasClave)
            : base(id)
        {
            DestinoId = destinoId;
            UsuarioId = usuarioId;
            Titulo = titulo;
            Descripcion = descripcion;
            Valoracion = valoracion;
            PalabrasClave = palabrasClave;
        }
    }
}
