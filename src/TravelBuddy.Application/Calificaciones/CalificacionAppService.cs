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
    [AllowAnonymous] // Permite probar todo en Swagger sin tokens
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

        [AllowAnonymous]
        public override async Task<CalificacionDto> GetAsync(Guid id)
        {
            return await base.GetAsync(id);
        }

        [AllowAnonymous]
        public override async Task<Volo.Abp.Application.Dtos.PagedResultDto<CalificacionDto>> GetListAsync(CalificacionGetListInput input)
        {
            return await base.GetListAsync(input);
        }

        [AllowAnonymous]
        public override async Task<CalificacionDto> CreateAsync(CreateUpdateCalificacionDto input)
        {
            // Usamos el ID del usuario actual, o un ID vacío temporal para poder probar en Swagger sin login
            var usuarioId = CurrentUser.Id ?? Guid.Empty;

            // Control de duplicados: se fija si este usuario ya calificó este destino
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

        [AllowAnonymous]
        public override async Task<CalificacionDto> UpdateAsync(Guid id, CreateUpdateCalificacionDto input)
        {
            var entity = await Repository.GetAsync(id);
            EnsureOwner(entity);

            entity.SetEstrellas(input.Estrellas);
            entity.SetComentario(input.Comentario);

            await Repository.UpdateAsync(entity, autoSave: true);
            return ObjectMapper.Map<Calificacion, CalificacionDto>(entity);
        }

        [AllowAnonymous]
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            EnsureOwner(entity);

            await Repository.DeleteAsync(entity, autoSave: true);
        }

        [AllowAnonymous]
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
            return query;
        }

        private void EnsureOwner(Calificacion entity)
        {
            // Desactivado temporalmente para pruebas locales sin login
            if (CurrentUser.Id.HasValue && entity.UsuarioId != CurrentUser.Id.Value)
            {
                throw new AbpAuthorizationException();
            }
        }
    }
}
