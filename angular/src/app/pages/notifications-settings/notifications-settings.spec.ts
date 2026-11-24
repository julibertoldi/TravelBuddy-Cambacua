import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationDto, NotificationService } from '../../shared/services/notification.service';
import { NotificationsSettings } from './notifications-settings';

describe('NotificationsSettings', () => {
  let component: NotificationsSettings;
  let fixture: ComponentFixture<NotificationsSettings>;
  let notificationService: jasmine.SpyObj<NotificationService>;

  beforeEach(async () => {
    notificationService = jasmine.createSpyObj<NotificationService>('NotificationService', [
      'getNotifications',
      'markAsRead',
      'getSettings',
      'updateSettings',
      'getUpcomingFavoriteEvents'
    ]);
    const notifications: NotificationDto[] = Array.from({ length: 6 }, (_, index) => ({
      id: `notification-${index}`,
      title: `Aviso ${index}`,
      message: `Mensaje ${index}`,
      isRead: false,
      creationTime: new Date(Date.UTC(2026, 0, index + 1)).toISOString()
    }));
    notificationService.getNotifications.and.returnValue(of(notifications));
    notificationService.markAsRead.and.returnValue(of(void 0));
    notificationService.getSettings.and.returnValue(of({
      emailNotificationsEnabled: true,
      newReviewNotificationsEnabled: false,
      upcomingEventsEnabled: true
    }));
    notificationService.updateSettings.and.returnValue(of(void 0));
    notificationService.getUpcomingFavoriteEvents.and.returnValue(of([
      { tituloEvento: 'Concierto', fechaEvento: '2026-06-01T20:00:00Z', nombreDestino: 'Buenos Aires' }
    ]));

    await TestBed.configureTestingModule({
      imports: [NotificationsSettings],
      providers: [{ provide: NotificationService, useValue: notificationService }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotificationsSettings);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows the newest notifications first and limits the recent view to five', () => {
    expect(component.visibleNotifications[0].id).toBe('notification-5');
    expect(component.visibleNotifications.length).toBe(5);
  });

  it('shows the full notification history', () => {
    component.notificationView = 'history';

    expect(component.visibleNotifications.length).toBe(6);
    expect(component.visibleNotifications[0].id).toBe('notification-5');
  });

  it('marks a notification as read and updates the unread count', () => {
    component.markAsRead('notification-5');

    expect(notificationService.markAsRead).toHaveBeenCalledWith('notification-5');
    expect(component.notifications.find(notification => notification.id === 'notification-5')?.isRead).toBeTrue();
    expect(component.unreadCount).toBe(5);
  });

  it('allows retrying a failed mark-as-read request', () => {
    spyOn(console, 'error');
    notificationService.markAsRead.and.returnValue(throwError(() => new Error('Request failed')));

    component.markAsRead('notification-5');

    expect(component.markingNotificationIds.has('notification-5')).toBeFalse();
    expect(component.notifications.find(notification => notification.id === 'notification-5')?.isRead).toBeFalse();
    expect(component.notificationActionError).toContain('No se pudo marcar');
  });

  it('loads and saves the user notification preferences', () => {
    expect(component.settings.newReviewNotificationsEnabled).toBeFalse();
    component.settings.emailNotificationsEnabled = false;

    component.saveSettings();

    expect(notificationService.updateSettings).toHaveBeenCalledWith(component.settings);
    expect(component.savedSuccess).toBeTrue();
  });

  it('loads and displays upcoming events alongside notifications', () => {
    fixture.detectChanges();

    expect(component.upcomingEvents[0].tituloEvento).toBe('Concierto');
    expect(fixture.nativeElement.textContent).toContain('Aviso 5');
    expect(fixture.nativeElement.textContent).toContain('Concierto');
  });
});
