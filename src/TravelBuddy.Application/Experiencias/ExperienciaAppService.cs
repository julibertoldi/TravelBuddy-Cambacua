using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace TravelBuddy.Experiencias
{
    public class ExperienciaAppService :
        CrudAppService<
            Experiencia,
            ExperienciaDto,
            Guid,
            ExperienciaGetListInput,
            CreateUpdateExperienciaDto>,
        IExperienciaAppService
    {
        public ExperienciaAppService(IRepository<Experiencia, Guid> repository)
            : base(repository)
        {
        }
        //Esto hace que el usuario no se mande desde Angular: el backend toma al usuario autenticado.
        public override async Task<ExperienciaDto> CreateAsync(
    CreateUpdateExperienciaDto input)
        {
            if (!CurrentUser.Id.HasValue)
            {
                throw new AbpAuthorizationException("Usuario no autenticado.");
            }

            var entity = new Experiencia(
                GuidGenerator.Create(),
                input.DestinoId,
                CurrentUser.Id.Value,
                input.Titulo,
                input.Descripcion,
                input.Valoracion,
                input.PalabrasClave ?? string.Empty
            );

            await Repository.InsertAsync(entity, autoSave: true);

            return ObjectMapper.Map<Experiencia, ExperienciaDto>(entity);
        }
        // Actualiza una experiencia existente.
        // Antes de modificarla verifica que el usuario autenticado
        // sea el propietario de la experiencia mediante EnsureOwner().
        public override async Task<ExperienciaDto> UpdateAsync(
    Guid id,
    CreateUpdateExperienciaDto input)
        {
            var entity = await Repository.GetAsync(id);

            EnsureOwner(entity);

            entity.DestinoId = input.DestinoId;
            entity.Titulo = input.Titulo;
            entity.Descripcion = input.Descripcion;
            entity.Valoracion = input.Valoracion;
            entity.PalabrasClave = input.PalabrasClave ?? string.Empty;

            await Repository.UpdateAsync(entity, autoSave: true);

            return ObjectMapper.Map<Experiencia, ExperienciaDto>(entity);
        }
        // Elimina una experiencia.
        // Antes de borrarla verifica que el usuario autenticado
        // sea el propietario de esa experiencia.
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            EnsureOwner(entity);

            await Repository.DeleteAsync(entity, autoSave: true);
        }

        // Esto evita que alguien modifique o borre experiencias ajenas.
        private void EnsureOwner(Experiencia entity)
        {
            if (!CurrentUser.Id.HasValue ||
                entity.UsuarioId != CurrentUser.Id.Value)
            {
                throw new AbpAuthorizationException(
                    "Solo podés modificar tus propias experiencias."
                );
            }
        }
        //construir la consulta según lo que el usuario complete en la pantalla de búsqueda
        protected override async Task<IQueryable<Experiencia>>
            CreateFilteredQueryAsync(ExperienciaGetListInput input)
        {
            var query = await Repository.GetQueryableAsync();

            return query
                .WhereIf(
                    input.DestinoId.HasValue,
                    x => x.DestinoId == input.DestinoId
                )
                .WhereIf(
                    input.Valoracion.HasValue,
                    x => x.Valoracion == input.Valoracion
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Keyword),
                    x =>
                        x.PalabrasClave.Contains(input.Keyword!) ||
                        x.Titulo.Contains(input.Keyword!) ||
                        x.Descripcion.Contains(input.Keyword!)
                );
        }
    }
}