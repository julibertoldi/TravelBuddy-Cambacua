using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using TravelBuddy.Calificaciones;
using TravelBuddy.Favorites;
using Volo.Abp.Domain.Repositories;
using TravelBuddy.Notificaciones;
using Volo.Abp.Uow;
using System.Linq;
using TravelBuddy.Destinations;
using Volo.Abp.Guids;
using Volo.Abp.Linq;

namespace TravelBuddy.Notifications
{
    public class ReviewCreatedEventHandler : ILocalEventHandler<EntityCreatedEventData<Calificacion>>, ITransientDependency
    {
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IGuidGenerator _guidGenerator;
        public IAsyncQueryableExecuter AsyncExecuter { get; set; }

        public ReviewCreatedEventHandler(
            IRepository<Favorite> favoriteRepository,
            IRepository<Notification, Guid> notificationRepository,
            IRepository<Destination, Guid> destinationRepository,
            IGuidGenerator guidGenerator)
        {
            _favoriteRepository = favoriteRepository;
            _notificationRepository = notificationRepository;
            _destinationRepository = destinationRepository;
            _guidGenerator = guidGenerator;
        }

        [UnitOfWork]
        public async Task HandleEventAsync(EntityCreatedEventData<Calificacion> eventData)
        {
            var review = eventData.Entity;

            var destination = await _destinationRepository.FirstOrDefaultAsync(d => d.Id == review.DestinoId);
            var destinationName = destination?.Name ?? "un destino";

            var query = await _favoriteRepository.GetQueryableAsync();
            var filteredQuery = query.Where(f => f.DestinoId == review.DestinoId && f.UsuarioId != review.UsuarioId);
            var favoriteUsers = await AsyncExecuter.ToListAsync(filteredQuery);

            foreach (var favorite in favoriteUsers)
            {
                var notification = new Notification(
                    _guidGenerator.Create(),
                    favorite.UsuarioId,
                    "Nueva reseña",
                    $"Alguien ha dejado una nueva calificación de {review.Estrellas} estrellas en {destinationName}."
                );

                await _notificationRepository.InsertAsync(notification);
            }
        }
    }
}
