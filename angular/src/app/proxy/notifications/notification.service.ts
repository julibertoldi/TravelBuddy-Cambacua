import type { NotificationDto, NotificationSettingsDto, UpcomingEventNotificationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  apiName = 'Default';
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, NotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification',
    },
    { apiName: this.apiName,...config });
  

  getSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, NotificationSettingsDto>({
      method: 'GET',
      url: '/api/app/notification/settings',
    },
    { apiName: this.apiName,...config });
  

  getUpcomingFavoriteEvents = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, UpcomingEventNotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification/upcoming-favorite-events',
    },
    { apiName: this.apiName,...config });
  

  markAsRead = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/notification/${id}/mark-as-read`,
    },
    { apiName: this.apiName,...config });
  

  updateSettings = (input: NotificationSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/notification/settings',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
