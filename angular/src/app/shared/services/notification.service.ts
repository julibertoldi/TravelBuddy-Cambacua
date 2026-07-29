import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface NotificationSettingsDto {
  receiveEventAlerts: boolean;
  receiveEmailNotifications: boolean;
  receivePromotions: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private restService: RestService) {}

  // 1. Notificaciones (OK)
  getNotifications(): Observable<any[]> {
    return this.restService.request<any, any[]>({
      method: 'GET',
      url: '/api/app/notification'
    });
  }

  // 1b. Marcar como leída
  markAsRead(id: string): Observable<void> {
    return this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/notification/${id}/mark-as-read`
    });
  }

  // 2. Configuración (OK)
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
  getFavoriteUpcomingEvents(): Observable<any[]> {
    return this.restService.request<any, any[]>({
      method: 'GET',
      url: '/api/app/notification/upcoming-favorite-events'
    });
  }
}