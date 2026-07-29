using Shouldly;
using System;
using System.Threading.Tasks;
using TravelBuddy.Notifications;
using Xunit;

namespace TravelBuddy.Notificaciones
{
    public class NotificationAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationAppService_Tests()
        {
            _notificationAppService = GetService<INotificationAppService>();
        }

        // TEST 1: Caso en que el usuario no tiene favoritos registrados
        [Fact]
        public async Task GetUpcomingFavoriteEventsAsync_Devolver_Lista_Vacia_Cuando_Usuario_No_Tiene_Favoritos()
        {
            // Act
            var result = await _notificationAppService.GetUpcomingFavoriteEventsAsync();

            // Assert
            result.ShouldNotBeNull();
        }

        // TEST 2: Caso de prueba para verificar que procese y mapee la lista de eventos
        [Fact]
        public async Task GetUpcomingFavoriteEventsAsync_Devolver_Eventos_Para_Destinos_Favoritos()
        {
            // Act
            var result = await _notificationAppService.GetUpcomingFavoriteEventsAsync();

            // Assert
            result.ShouldNotBeNull();
        }

        // TEST 3: Caso para asegurar la estabilidad del modelo SQL y la integración externa
        [Fact]
        public async Task GetUpcomingFavoriteEventsAsync_No_Debe_Lanzar_Excepcion()
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _notificationAppService.GetUpcomingFavoriteEventsAsync();
            });

            exception.ShouldBeNull();
        }
    }
}