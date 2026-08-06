import type { EntityDto } from '@abp/ng.core';

export interface NotificationDto extends EntityDto<string> {
  title?: string;
  message?: string;
  isRead: boolean;
  creationTime?: string;
}

export interface NotificationSettingsDto {
  emailNotificationsEnabled: boolean;
  newReviewNotificationsEnabled: boolean;
  upcomingEventsEnabled: boolean;
}

export interface UpcomingEventNotificationDto {
  destinoId?: string;
  nombreDestino?: string;
  tituloEvento?: string;
  categoria?: string;
  fechaEvento?: string;
  eventUrl?: string;
}
