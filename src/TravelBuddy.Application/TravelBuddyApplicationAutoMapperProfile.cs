using AutoMapper;

namespace TravelBuddy;

public class TravelBuddyApplicationAutoMapperProfile : Profile
{
    public TravelBuddyApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Destinations.Destination, Destinations.DestinationDto>();
        CreateMap<Destinations.CreateUpdateDestinationDto, Destinations.Destination>();

        CreateMap<Experiencias.Experiencia, Experiencias.ExperienciaDto>();
        CreateMap<Experiencias.CreateUpdateExperienciaDto, Experiencias.Experiencia>();

        CreateMap<Calificaciones.Calificacion, Calificaciones.CalificacionDto>();
        CreateMap<Calificaciones.CreateUpdateCalificacionDto, Calificaciones.Calificacion>();
    }
}
