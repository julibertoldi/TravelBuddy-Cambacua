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
    const recentNotifications: NotificationDto[] = Array.from({ length: 6 }, (_, index) => ({
      id: `notification-${index}`,
      title: `Aviso ${index}`,
      message: `Mensaje ${index}`,
      isRead: false,
      creationTime: new Date(Date.now() - index * 24 * 60 * 60 * 1000).toISOString()
    }));
    const notifications: NotificationDto[] = [
      ...recentNotifications,
      {
        id: 'old-notification',
        title: 'Aviso antiguo',
        message: 'Mensaje antiguo',
        isRead: false,
        creationTime: new Date(Date.now() - 8 * 24 * 60 * 60 * 1000).toISOString()
      },
      {
        id: 'read-notification',
        title: 'Aviso leído',
        message: 'Mensaje leído',
        isRead: true,
        creationTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString()
      }
    ];
    notificationService.getNotifications.and.returnValue(of(notifications));
    notificationService.markAsRead.and.returnValue(of(void 0));
    notificationService.getSettings.and.returnValue(of({
      emailNotificationsEnabled: false,
      newReviewNotificationsEnabled: false,
      upcomingEventsEnabled: false
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

  it('shows all unread notifications from the last week, newest first', () => {
    expect(component.visibleNotifications[0].id).toBe('notification-0');
    expect(component.visibleNotifications.length).toBe(6);
    expect(component.visibleNotifications.some(notification => notification.id === 'old-notification')).toBeFalse();
    expect(component.visibleNotifications.some(notification => notification.id === 'read-notification')).toBeFalse();
    expect(component.unreadCount).toBe(6);
  });

  it('shows the full notification history', () => {
    component.notificationView = 'history';

    expect(component.visibleNotifications.length).toBe(8);
    expect(component.visibleNotifications[0].id).toBe('notification-0');
  });

  it('marks a notification as read and updates the unread count', () => {
    component.markAsRead('notification-5');

    expect(notificationService.markAsRead).toHaveBeenCalledWith('notification-5');
    expect(component.notifications.find(notification => notification.id === 'notification-5')?.isRead).toBeTrue();
    expect(component.unreadCount).toBe(5);
    expect(component.visibleNotifications.some(notification => notification.id === 'notification-5')).toBeFalse();
  });

  it('allows retrying a failed mark-as-read request', () => {
    spyOn(console, 'error');
    notificationService.markAsRead.and.returnValue(throwError(() => new Error('Request failed')));

    component.markAsRead('notification-5');

    expect(component.markingNotificationIds.has('notification-5')).toBeFalse();
    expect(component.notifications.find(notification => notification.id === 'notification-5')?.isRead).toBeFalse();
    expect(component.notificationActionError).toContain('No se pudo marcar');
  });

  it('starts disabled and enables saving only after settings change', () => {
    expect(component.settings.newReviewNotificationsEnabled).toBeFalse();
    expect(component.settings.emailNotificationsEnabled).toBeFalse();
    expect(component.settings.upcomingEventsEnabled).toBeFalse();
    expect(component.hasUnsavedSettingsChanges).toBeFalse();

    component.settings.emailNotificationsEnabled = true;
    component.onSettingsChanged();
    expect(component.hasUnsavedSettingsChanges).toBeTrue();

    fixture.detectChanges();
    const saveButton = fixture.nativeElement.querySelector('.btn-save') as HTMLButtonElement;
    expect(saveButton.disabled).toBeFalse();

    component.saveSettings();

    expect(notificationService.updateSettings).toHaveBeenCalledWith(component.settings);
    expect(component.savedSuccess).toBeTrue();
    expect(component.hasUnsavedSettingsChanges).toBeFalse();
    fixture.detectChanges();
    expect(saveButton.disabled).toBeTrue();
  });

  it('loads and displays upcoming events alongside notifications', () => {
    fixture.detectChanges();

    expect(component.upcomingEvents[0].tituloEvento).toBe('Concierto');
    expect(fixture.nativeElement.textContent).toContain('Aviso 5');
    expect(fixture.nativeElement.textContent).toContain('Concierto');
  });
});
