using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Calificaciones
{
    public interface ICalificacionAppService :
        ICrudAppService<CalificacionDto, Guid, CalificacionGetListInput, CreateUpdateCalificacionDto>
    {
        Task<CalificacionPromedioDto> GetPromedioByDestinoAsync(Guid destinoId);
    }
}
