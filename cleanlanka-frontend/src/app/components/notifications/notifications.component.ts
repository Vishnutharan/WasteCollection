import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  http = inject(HttpClient);
  notifications: any[] = [];
  apiUrl = 'http://localhost:5214/api/notifications';

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => this.notifications = data,
      error: (err) => console.error(err)
    });
  }

  markAsRead(id: number) {
     // TODO: Implement mark as read endpoint if available
  }
}
