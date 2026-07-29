import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, UpcomingEventNotificationDto } from '../../services/notification.service';

@Component({
  selector: 'app-notifications-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications-dropdown.html',
  styleUrl: './notifications-dropdown.scss'
})
export class NotificationsDropdown implements OnInit {
  events: UpcomingEventNotificationDto[] = [];
  isOpen = false;
  loading = false;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  loadNotifications(): void {
    this.loading = true;
    this.notificationService.getUpcomingFavoriteEvents().subscribe({
      next: (data) => {
        this.events = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error al cargar notificaciones', err);
        this.loading = false;
      }
    });
  }
}
