using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace TravelBuddy.Calificaciones
{
    [Authorize] // Exigir autenticación para proteger la privacidad
    public class CalificacionAppService :
        CrudAppService<
            Calificacion,
            CalificacionDto,
            Guid,
            CalificacionGetListInput,
            CreateUpdateCalificacionDto>,
        ICalificacionAppService
    {
        public CalificacionAppService(IRepository<Calificacion, Guid> repository)
            : base(repository)
        {
        }

        public override async Task<CalificacionDto> CreateAsync(CreateUpdateCalificacionDto input)
        {
            var usuarioId = CurrentUser.Id ?? throw new UserFriendlyException("Debe iniciar sesión para calificar.");

            // Control de duplicados: una sola reseña por usuario por destino
            var exists = await Repository.AnyAsync(x => x.DestinoId == input.DestinoId && x.UsuarioId == usuarioId);
            if (exists)
            {
                throw new BusinessException("TravelBuddy:Calificacion:YaExiste");
            }

            var entity = new Calificacion(
                GuidGenerator.Create(),
                input.DestinoId,
                usuarioId,
                input.Estrellas,
                input.Comentario
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return ObjectMapper.Map<Calificacion, CalificacionDto>(entity);
        }

        public override async Task<CalificacionDto> UpdateAsync(Guid id, CreateUpdateCalificacionDto input)
        {
            var entity = await Repository.GetAsync(id);
            EnsureOwner(entity);

            entity.SetEstrellas(input.Estrellas);
            entity.SetComentario(input.Comentario);

            await Repository.UpdateAsync(entity, autoSave: true);
            return ObjectMapper.Map<Calificacion, CalificacionDto>(entity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            EnsureOwner(entity);

            await Repository.DeleteAsync(entity, autoSave: true);
        }
        public async Task<CalificacionPromedioDto> GetPromedioByDestinoAsync(Guid destinoId)
        {
            var query = await Repository.GetQueryableAsync();
            var filteredQuery = query.Where(x => x.DestinoId == destinoId)
                .GroupBy(x => 1)
                .Select(g => new CalificacionPromedioDto
                {
                    Promedio = g.Average(x => (double)x.Estrellas),
                    TotalCalificaciones = g.Count()
                });

            var result = await AsyncExecuter.FirstOrDefaultAsync(filteredQuery);
            return result ?? new CalificacionPromedioDto();
        }

        protected override async Task<IQueryable<Calificacion>> CreateFilteredQueryAsync(CalificacionGetListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (input.DestinoId.HasValue)
            {
                query = query.Where(x => x.DestinoId == input.DestinoId.Value);
            }

            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                // Solo muestra la reseña perteneciente al usuario logueado
                query = query.Where(x => x.UsuarioId == userId.Value);
            }
            else
            {
                query = query.Where(x => false);
            }

            return query;
        }

        private void EnsureOwner(Calificacion entity)
        {
            if (CurrentUser.Id.HasValue && entity.UsuarioId != CurrentUser.Id.Value)
            {
                throw new AbpAuthorizationException();
            }
        }
    }
}