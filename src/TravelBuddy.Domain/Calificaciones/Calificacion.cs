using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelBuddy.Calificaciones
{
    public class Calificacion : AuditedAggregateRoot<Guid>
    {
        public Guid DestinoId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public int Estrellas { get; private set; }
        public string Comentario { get; private set; } = string.Empty;

        protected Calificacion()
        {
        }

        public Calificacion(Guid id, Guid destinoId, Guid usuarioId, int estrellas, string comentario) : base(id)
        {
            DestinoId = destinoId;
            UsuarioId = usuarioId;
            SetEstrellas(estrellas);
            SetComentario(comentario);
        }

        public void SetEstrellas(int estrellas)
        {
            if (estrellas is < 1 or > 5) throw new BusinessException("TravelBuddy:Calificacion:EstrellasInvalidas");
            Estrellas = estrellas;
        }

        public void SetComentario(string comentario) => Comentario = Check.NotNullOrWhiteSpace(comentario, nameof(comentario), maxLength: 2000);
    }
}
