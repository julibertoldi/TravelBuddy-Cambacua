using AutoMapper;
using TravelBuddy.Destinations;

namespace TravelBuddy;

public class TravelBuddyApplicationAutoMapperProfile : Profile
{
    public TravelBuddyApplicationAutoMapperProfile()
    {
        CreateMap<Destination, DestinationDto>();
        CreateMap<CreateUpdateDestinationDto, Destination>();

        CreateMap<Experiencias.Experiencia, Experiencias.ExperienciaDto>();
        CreateMap<Experiencias.CreateUpdateExperienciaDto, Experiencias.Experiencia>();
    }
}