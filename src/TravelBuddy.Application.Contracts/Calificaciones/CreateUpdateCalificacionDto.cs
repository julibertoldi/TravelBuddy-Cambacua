using System;
using System.ComponentModel.DataAnnotations;

namespace TravelBuddy.Calificaciones
{
    public class CreateUpdateCalificacionDto
    {
        [Required]
        public Guid DestinoId { get; set; }

        [Range(1, 5)]
        public int Estrellas { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Comentario { get; set; } = string.Empty;

        public string PalabrasClave { get; set; } = string.Empty;
    }
}
