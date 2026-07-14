using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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
    }
}
