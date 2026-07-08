using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Experiencias
{
    public interface IExperienciaAppService : 
        ICrudAppService< // Defines CRUD methods
            ExperienciaDto, // Used to show entities
            Guid, // Primary key of the entity
            PagedAndSortedResultRequestDto, // Used for paging/sorting
            CreateUpdateExperienciaDto> // Used to create/update an entity
    {
    }
}
