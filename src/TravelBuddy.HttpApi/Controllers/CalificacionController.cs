using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using TravelBuddy.Calificaciones;

namespace TravelBuddy.Controllers
{
    [RemoteService]
    [Area("app")]
    [Route("api/app/calificaciones-custom")] // Cambiamos temporalmente la ruta base para que no choque con el CrudAppService
    [Authorize]
    public class CalificacionController : AbpController
    {
        private readonly ICalificacionAppService _calificacionAppService;

        public CalificacionController(ICalificacionAppService calificacionAppService)
        {
            _calificacionAppService = calificacionAppService;
        }

        [HttpGet("promedio-por-destino")] // Ruta clara sin guiones extraños que confundan al binder de GUIDs
        public async Task<CalificacionPromedioDto> GetPromedioByDestinoAsync(Guid destinoId)
        {
            return await _calificacionAppService.GetPromedioByDestinoAsync(destinoId);
        }
    }
}