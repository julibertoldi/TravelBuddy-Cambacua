import type { NotificationDto, NotificationSettingsDto, UpcomingEventNotificationDto } from '../../proxy/notifications/models';
import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private restService: RestService) {}

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

  getFavoriteUpcomingEvents(): Observable<UpcomingEventNotificationDto[]> {
    return this.restService.request<any, UpcomingEventNotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification/upcoming-favorite-events'
    });
  }
}