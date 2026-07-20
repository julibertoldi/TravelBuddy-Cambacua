using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Experiencias
{
    public interface IExperienciaAppService : 
        ICrudAppService< // Defines CRUD methods
            ExperienciaDto, // Used to show entities
            Guid, // Primary key of the entity
            ExperienciaGetListInput, // para aceptar los filtr definidos en ExperienciaGetListInput
            CreateUpdateExperienciaDto> // Used to create/update an entity
    {
    }
}
