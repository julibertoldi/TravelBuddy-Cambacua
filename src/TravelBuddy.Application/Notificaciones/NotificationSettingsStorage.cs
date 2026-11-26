using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TravelBuddy.Notifications
{
    internal sealed class NotificationSettingsStorage : NotificationSettingsDto
    {
        public List<PendingNotification> PendingWeeklyNotifications { get; set; } = new();
        public DateTime? FirstPendingWeeklyNotificationAt { get; set; }
        public DateTime? LastWeeklySummaryDate { get; set; }

        public static NotificationSettingsStorage Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new NotificationSettingsStorage();
            }

            try
            {
                var settings = JsonSerializer.Deserialize<NotificationSettingsStorage>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new NotificationSettingsStorage();
                settings.Frequency = NormalizeFrequency(settings.Frequency);
                settings.PendingWeeklyNotifications ??= new List<PendingNotification>();
                return settings;
            }
            catch (JsonException)
            {
                return new NotificationSettingsStorage();
            }
        }

        public NotificationSettingsDto ToDto()
        {
            return new NotificationSettingsDto
            {
                EmailNotificationsEnabled = EmailNotificationsEnabled,
                NewReviewNotificationsEnabled = NewReviewNotificationsEnabled,
                UpcomingEventsEnabled = UpcomingEventsEnabled,
                Frequency = NormalizeFrequency(Frequency)
            };
        }

        public void UpdateFrom(NotificationSettingsDto input)
        {
            EmailNotificationsEnabled = input.EmailNotificationsEnabled;
            NewReviewNotificationsEnabled = input.NewReviewNotificationsEnabled;
            UpcomingEventsEnabled = input.UpcomingEventsEnabled;
            Frequency = NormalizeFrequency(input.Frequency);
        }

        public static string NormalizeFrequency(string? frequency)
        {
            return string.Equals(frequency, "weekly", StringComparison.OrdinalIgnoreCase)
                ? "weekly"
                : "immediate";
        }
    }

    internal sealed class PendingNotification
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}