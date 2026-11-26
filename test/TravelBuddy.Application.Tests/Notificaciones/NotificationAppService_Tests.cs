using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelBuddy.Notificaciones;
using TravelBuddy.Notifications;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace TravelBuddy.Notificaciones
{
    public class NotificationAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
    {
        private readonly INotificationAppService _notificationAppService;
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly Guid _userId = Guid.NewGuid();

        public NotificationAppService_Tests()
        {
            _notificationAppService = GetRequiredService<INotificationAppService>();
            _notificationRepository = GetRequiredService<IRepository<Notification, Guid>>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        [Fact]
        public async Task GetListAsync_Solo_Devuelve_Notificaciones_Del_Usuario_Actual()
        {
            await _notificationRepository.InsertAsync(new Notification(Guid.NewGuid(), _userId, "Propia", "Mensaje propio"));
            await _notificationRepository.InsertAsync(new Notification(Guid.NewGuid(), Guid.NewGuid(), "Ajena", "Mensaje ajeno"));

            using (LoginAs(_userId))
            {
                var result = await _notificationAppService.GetListAsync();

                result.ShouldHaveSingleItem().Title.ShouldBe("Propia");
            }
        }

        [Fact]
        public async Task GetUpcomingFavoriteEventsAsync_Requiere_Usuario_Autenticado()
        {
            await Should.ThrowAsync<AbpAuthorizationException>(
                () => _notificationAppService.GetUpcomingFavoriteEventsAsync());
        }

        private IDisposable LoginAs(Guid userId)
        {
            var claims = new List<Claim>
            {
                new(AbpClaimTypes.UserId, userId.ToString()),
                new(AbpClaimTypes.UserName, "notification-user")
            };

            return _currentPrincipalAccessor.Change(
                new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }
    }
}