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
    upcomingEventsEnabled: true,
    emailNotificationsEnabled: true,
    newReviewNotificationsEnabled: true
  };
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
    return this.notificationView === 'recent' ? this.notifications.slice(0, 5) : this.notifications;
  }

  get unreadCount(): number {
    return this.notifications.filter(notification => !notification.isRead).length;
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
        if (data) this.settings = { ...this.settings, ...data };
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
    this.loadingSettings = true;
    this.savedSuccess = false;
    this.settingsSaveError = '';
    this.notificationService.updateSettings(this.settings).subscribe({
      next: () => {
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