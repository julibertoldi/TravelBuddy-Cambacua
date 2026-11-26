using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp.Uow;

namespace TravelBuddy.Notifications
{
    public class NotificationBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<NotificationBackgroundWorker> _logger;

        public NotificationBackgroundWorker(
            IServiceScopeFactory scopeFactory,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<NotificationBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
                    var processor = scope.ServiceProvider.GetRequiredService<NotificationPeriodicProcessor>();
                    await processor.RunDailyAsync();
                    await unitOfWork.CompleteAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Falló el ciclo diario de notificaciones.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}