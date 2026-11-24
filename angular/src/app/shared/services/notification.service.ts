import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import type { NotificationDto, NotificationSettingsDto, UpcomingEventNotificationDto } from '../../proxy/notifications/models';

export type { NotificationDto, NotificationSettingsDto, UpcomingEventNotificationDto } from '../../proxy/notifications/models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private restService: RestService) {}

  // 1. Notificaciones (OK)
  getNotifications(): Observable<NotificationDto[]> {
    return this.restService.request<any, NotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification'
    });
  }

  markAsRead(id: string): Observable<void> {
    return this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/notification/${id}/mark-as-read`
    });
  }

  getSettings(): Observable<NotificationSettingsDto> {
    return this.restService.request<any, NotificationSettingsDto>({
      method: 'GET',
      url: '/api/app/notification/settings'
    });
  }

  updateSettings(settings: NotificationSettingsDto): Observable<void> {
    return this.restService.request<NotificationSettingsDto, void>({
      method: 'PUT',
      url: '/api/app/notification/settings',
      body: settings
    });
  }

  // 3. PRUEBA NUEVA: Eventos favoritos próximos
  getUpcomingFavoriteEvents(): Observable<UpcomingEventNotificationDto[]> {
    return this.restService.request<any, UpcomingEventNotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification/upcoming-favorite-events'
    });
  }
}