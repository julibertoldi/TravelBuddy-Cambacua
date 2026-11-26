using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TravelBuddy.Notificaciones;
using TravelBuddy.Users;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace TravelBuddy.Notifications
{
    public class NotificationDeliveryService : ITransientDependency
    {
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<NotificationDeliveryService> _logger;

        public NotificationDeliveryService(
            IRepository<Notification, Guid> notificationRepository,
            IdentityUserManager userManager,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<NotificationDeliveryService> logger)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        public async Task EnqueueAsync(
            Guid userId,
            string title,
            string message,
            bool onlyWhenEventsEnabled = false,
            bool onlyWhenReviewsEnabled = false)
        {
            var user = await _userManager.GetByIdAsync(userId);
            var preferencesJson = user.GetNotificationSettings() ?? user.GetPreferences();
            var settings = NotificationSettingsStorage.Deserialize(preferencesJson);

            if (onlyWhenEventsEnabled && !settings.UpcomingEventsEnabled)
            {
                return;
            }

            if (onlyWhenReviewsEnabled && !settings.NewReviewNotificationsEnabled)
            {
                return;
            }

            if (settings.Frequency == "weekly")
            {
                if (!settings.PendingWeeklyNotifications.Any(item => item.Title == title && item.Message == message))
                {
                    if (!settings.PendingWeeklyNotifications.Any())
                    {
                        settings.FirstPendingWeeklyNotificationAt = DateTime.UtcNow;
                    }

                    settings.PendingWeeklyNotifications.Add(new PendingNotification { Title = title, Message = message });
                    user.SetNotificationSettings(System.Text.Json.JsonSerializer.Serialize(settings));
                    await _userManager.UpdateAsync(user);
                }

                return;
            }

            var existing = await _notificationRepository.FirstOrDefaultAsync(notification =>
                notification.UserId == userId && notification.Title == title && notification.Message == message);
            if (existing != null)
            {
                return;
            }

            await CreateNotificationAsync(userId, title, message);
            await SendEmailIfEnabledAsync(user.Email, settings, title, message);
        }

        public async Task DeliverWeeklySummaryAsync(Guid userId, DateTime date)
        {
            var user = await _userManager.GetByIdAsync(userId);
            var settings = NotificationSettingsStorage.Deserialize(user.GetNotificationSettings() ?? user.GetPreferences());
            if (!settings.PendingWeeklyNotifications.Any()
                || !settings.FirstPendingWeeklyNotificationAt.HasValue
                || date.Date < settings.FirstPendingWeeklyNotificationAt.Value.Date.AddDays(7)
                || settings.LastWeeklySummaryDate?.Date == date.Date)
            {
                return;
            }

            var summary = string.Join(Environment.NewLine, settings.PendingWeeklyNotifications
                .Select(item => $"{item.Title}: {item.Message}"));
            const string title = "Resumen semanal de notificaciones";
            await CreateNotificationAsync(userId, title, summary);
            await SendEmailIfEnabledAsync(user.Email, settings, title, summary);

            settings.PendingWeeklyNotifications.Clear();
            settings.FirstPendingWeeklyNotificationAt = null;
            settings.LastWeeklySummaryDate = date.Date;
            user.SetNotificationSettings(System.Text.Json.JsonSerializer.Serialize(settings));
            await _userManager.UpdateAsync(user);
        }

        private async Task CreateNotificationAsync(Guid userId, string title, string message)
        {
            await _notificationRepository.InsertAsync(
                new Notification(_guidGenerator.Create(), userId, title, message));
        }

        private async Task SendEmailIfEnabledAsync(string? email, NotificationSettingsStorage settings, string title, string message)
        {
            if (settings.EmailNotificationsEnabled && !string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _emailSender.SendAsync(email, title, message);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "No se pudo enviar por email la notificación a {Email}.", email);
                }
            }
        }
    }
}