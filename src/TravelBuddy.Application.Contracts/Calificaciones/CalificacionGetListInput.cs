using System;
using Volo.Abp.Application.Dtos;

namespace TravelBuddy.Calificaciones
{
    public class CalificacionGetListInput : PagedAndSortedResultRequestDto
    {
        public Guid? DestinoId { get; set; }
    }
}
