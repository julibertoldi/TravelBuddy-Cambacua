using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Destinations
{
    public interface IDestinationAppService : 
        ICrudAppService< //Defines CRUD methods
            DestinationDto, //Used to show entities
            Guid, //Primary key of the entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            CreateUpdateDestinationDto> //Used to create/update an entity
    {
    
    }
}
