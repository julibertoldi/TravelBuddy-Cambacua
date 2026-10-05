using AutoMapper;
using TravelBuddy.Destinations;

namespace TravelBuddy;

public class TravelBuddyApplicationAutoMapperProfile : Profile
{
    public TravelBuddyApplicationAutoMapperProfile()
    {
        CreateMap<Destination, DestinationDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Ubicacion, opt => opt.MapFrom(src => src.Region))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.ImagenUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => src.IsAvailable));

        CreateMap<CreateUpdateDestinationDto, Destination>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Descripcion))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Ubicacion))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Precio))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImagenUrl))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Disponible));

        CreateMap<Experiencias.Experiencia, Experiencias.ExperienciaDto>();
        CreateMap<Experiencias.CreateUpdateExperienciaDto, Experiencias.Experiencia>();

        CreateMap<Calificaciones.Calificacion, Calificaciones.CalificacionDto>();
        CreateMap<Calificaciones.CreateUpdateCalificacionDto, Calificaciones.Calificacion>();

        CreateMap<Notificaciones.Notification, Notifications.NotificationDto>();
    }
}