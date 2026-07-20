using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Calificaciones
{
    public class CalificacionDto : AuditedEntityDto<Guid>
    {
        public Guid DestinoId { get; set; }
        public Guid UsuarioId { get; set; }
        public int Estrellas { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}
