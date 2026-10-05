using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;
using TravelBuddy.Calificaciones;

namespace TravelBuddy.Controllers
{
    [RemoteService]
    [Area("app")]
    [Route("api/app/calificaciones-custom")] // Cambiamos temporalmente la ruta base para que no choque con el CrudAppService
    [AllowAnonymous]
    public class CalificacionController : AbpController
    {
        private readonly IRepository<Calificacion, Guid> _calificacionRepository;

        public CalificacionController(IRepository<Calificacion, Guid> calificacionRepository)
        {
            _calificacionRepository = calificacionRepository;
        }

        [HttpGet("promedio-por-destino")] // Ruta clara sin guiones extraños que confundan al binder de GUIDs
        public async Task<CalificacionPromedioDto> GetPromedioByDestinoAsync(Guid destinoId)
        {
            var query = await _calificacionRepository.GetQueryableAsync();

            var calificacionesDestino = query.Where(x => x.DestinoId == destinoId).ToList();

            if (!calificacionesDestino.Any())
            {
                return new CalificacionPromedioDto();
            }

            return new CalificacionPromedioDto
            {
                Promedio = calificacionesDestino.Average(x => (double)x.Estrellas),
                TotalCalificaciones = calificacionesDestino.Count
            };
        }
    }
}