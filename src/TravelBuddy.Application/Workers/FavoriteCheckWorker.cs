using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TravelBuddy.Favorites;
using TravelBuddy.Notificaciones;
using TravelBuddy.Notifications;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
namespace TravelBuddy.Workers
{
    public class FavoriteCheckWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public FavoriteCheckWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
        {
            // Configurar para que corra cada 24 horas (86400000 ms)
            // Para la prueba locales, 60000 ms (1 minuto)
            Timer.Period = 86400000;
        }
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            // Obtenemos el servicio a traves del contexto inyectado
            var notificationService = workerContext.ServiceProvider
            .GetRequiredService<INotificationAppService>();
            // Llamamos al metodo del servicio que orquesta la verificacion.
            // Crear 'CheckDailyFavoritesAsync' en INotificationAppService si no existe.
            await notificationService.CheckDailyFavoritesAsync();
        }
    }
}