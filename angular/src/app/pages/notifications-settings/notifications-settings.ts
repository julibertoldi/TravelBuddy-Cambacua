import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService, NotificationSettingsDto } from '../../shared/services/notification.service';

@Component({
  selector: 'app-notifications-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notifications-settings.html',
  styleUrl: './notifications-settings.scss'
})
export class NotificationsSettings implements OnInit {
  notifications: any[] = [];
  upcomingEvents: any[] = [];
  settings: NotificationSettingsDto = {
    receiveEventAlerts: true,
    receiveEmailNotifications: true,
    receivePromotions: false
  };

  loadingNotifications = false;
  loadingEvents = false;
  loadingSettings = false;
  savedSuccess = false;

  constructor(private notificationService: NotificationService) {}

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
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data || [];
        this.loadingNotifications = false;
      },
      error: (err) => {
        console.error('Error al obtener notificaciones', err);
        this.loadingNotifications = false;
      }
    });
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        const item = this.notifications.find(n => n.id === id);
        if (item) item.isRead = true;
      },
      error: (err) => console.error('Error al marcar como leída', err)
    });
  }

  loadSettings(): void {
    this.loadingSettings = true;
    this.notificationService.getSettings().subscribe({
      next: (data) => {
        if (data) this.settings = data;
        this.loadingSettings = false;
      },
      error: (err) => {
        console.error('Error al obtener configuración', err);
        this.loadingSettings = false;
      }
    });
  }

  saveSettings(): void {
    this.loadingSettings = true;
    this.savedSuccess = false;
    this.notificationService.updateSettings(this.settings).subscribe({
      next: () => {
        this.loadingSettings = false;
        this.savedSuccess = true;
        setTimeout(() => (this.savedSuccess = false), 3000);
      },
      error: (err) => {
        console.error('Error al guardar configuración', err);
        this.loadingSettings = false;
      }
    });
  }

  getUpcomingEvents(): void {
    this.loadingEvents = true;
    this.notificationService.getFavoriteUpcomingEvents().subscribe({
      next: (data) => {
        this.upcomingEvents = data || [];
        this.loadingEvents = false;
      },
      error: (err) => {
        console.error('Error al obtener eventos próximos', err);
        this.loadingEvents = false;
      }
    });
  }
}