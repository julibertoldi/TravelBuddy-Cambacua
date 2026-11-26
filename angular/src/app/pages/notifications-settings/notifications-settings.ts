import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  NotificationDto,
  NotificationService,
  NotificationSettingsDto,
  UpcomingEventNotificationDto
} from '../../shared/services/notification.service';

@Component({
  selector: 'app-notifications-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notifications-settings.html',
  styleUrl: './notifications-settings.scss'
})
export class NotificationsSettings implements OnInit {
  notifications: NotificationDto[] = [];
  upcomingEvents: UpcomingEventNotificationDto[] = [];
  settings: NotificationSettingsDto = {
    upcomingEventsEnabled: false,
    emailNotificationsEnabled: false,
    newReviewNotificationsEnabled: false,
    frequency: 'immediate'
  };
  private savedSettings: NotificationSettingsDto = { ...this.settings };
  notificationView: 'recent' | 'history' = 'recent';

  loadingNotifications = false;
  loadingEvents = false;
  loadingSettings = false;
  savedSuccess = false;
  notificationsError = '';
  notificationActionError = '';
  eventsError = '';
  settingsError = '';
  settingsSaveError = '';
  markingNotificationIds = new Set<string>();

  constructor(private notificationService: NotificationService) {}

  get visibleNotifications(): NotificationDto[] {
    return this.notificationView === 'recent' ? this.recentUnreadNotifications : this.notifications;
  }

  get unreadCount(): number {
    return this.recentUnreadNotifications.length;
  }

  get hasUnsavedSettingsChanges(): boolean {
    return this.settings.upcomingEventsEnabled !== this.savedSettings.upcomingEventsEnabled
      || this.settings.emailNotificationsEnabled !== this.savedSettings.emailNotificationsEnabled
      || this.settings.newReviewNotificationsEnabled !== this.savedSettings.newReviewNotificationsEnabled
      || this.settings.frequency !== this.savedSettings.frequency;
  }

  private get recentUnreadNotifications(): NotificationDto[] {
    const now = Date.now();
    const oneWeekAgo = now - 7 * 24 * 60 * 60 * 1000;

    return this.notifications.filter(notification => {
      const creationTime = new Date(notification.creationTime).getTime();
      return !notification.isRead && creationTime >= oneWeekAgo && creationTime <= now;
    });
  }

  ngOnInit(): void {
    this.loadAllData();
  }

  loadAllData(): void {
    this.getNotifications();
    this.loadSettings();
    this.getUpcomingEvents();
  }

  getNotifications(): void {
    this.loadingNotifications = true;
    this.notificationsError = '';
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = [...(data || [])].sort((first, second) =>
          new Date(second.creationTime || 0).getTime() - new Date(first.creationTime || 0).getTime()
        );
        this.loadingNotifications = false;
      },
      error: (err) => {
        console.error('Error al obtener notificaciones', err);
        this.notificationsError = 'No se pudieron cargar las notificaciones. Intenta nuevamente.';
        this.loadingNotifications = false;
      }
    });
  }

  markAsRead(id: string): void {
    if (this.markingNotificationIds.has(id)) return;

    this.notificationActionError = '';
    this.markingNotificationIds.add(id);
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        const item = this.notifications.find(n => n.id === id);
        if (item) item.isRead = true;
      },
      error: (err) => {
        console.error('Error al marcar como leída', err);
        this.markingNotificationIds.delete(id);
        this.notificationActionError = 'No se pudo marcar la notificación como leída. Intenta nuevamente.';
      },
      complete: () => this.markingNotificationIds.delete(id)
    });
  }

  loadSettings(): void {
    this.loadingSettings = true;
    this.settingsError = '';
    this.notificationService.getSettings().subscribe({
      next: (data) => {
        this.settings = { ...this.settings, ...(data || {}) };
        this.savedSettings = { ...this.settings };
        this.loadingSettings = false;
      },
      error: (err) => {
        console.error('Error al obtener configuración', err);
        this.settingsError = 'No se pudieron cargar tus preferencias.';
        this.loadingSettings = false;
      }
    });
  }

  saveSettings(): void {
    if (this.loadingSettings || !this.hasUnsavedSettingsChanges) return;

    this.loadingSettings = true;
    this.savedSuccess = false;
    this.settingsSaveError = '';
    this.notificationService.updateSettings(this.settings).subscribe({
      next: () => {
        this.savedSettings = { ...this.settings };
        this.loadingSettings = false;
        this.savedSuccess = true;
        setTimeout(() => (this.savedSuccess = false), 3000);
      },
      error: (err) => {
        console.error('Error al guardar configuración', err);
        this.settingsSaveError = 'No se pudieron guardar tus preferencias. Intenta nuevamente.';
        this.loadingSettings = false;
      }
    });
  }

  onSettingsChanged(): void {
    this.savedSuccess = false;
    this.settingsSaveError = '';
  }

  getUpcomingEvents(): void {
    this.loadingEvents = true;
    this.eventsError = '';
    this.notificationService.getUpcomingFavoriteEvents().subscribe({
      next: (data) => {
        this.upcomingEvents = data || [];
        this.loadingEvents = false;
      },
      error: (err) => {
        console.error('Error al obtener eventos próximos', err);
        this.eventsError = 'No se pudieron cargar los eventos próximos.';
        this.loadingEvents = false;
      }
    });
  }
}