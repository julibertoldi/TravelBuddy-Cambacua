using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Experiencias
{
    public class ExperienciaGetListInput : PagedAndSortedResultRequestDto
    {
        public Guid? DestinoId { get; set; }

        public ExperienciaValoracion? Valoracion { get; set; }

        public string? Keyword { get; set; }
    }
}