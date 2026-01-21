import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  http = inject(HttpClient);
  
  user: any;
  stats = {
    totalRequests: 0,
    pendingRequests: 0,
    activeRoutes: 0,
    completedRequests: 0
  };
  recentActivities: any[] = [];
  
  ngOnInit() {
    this.user = this.authService.getUser();
    this.loadStats();
    this.loadActivities();
  }
  
  loadStats() {
    // Mock data for now, replace with API calls
    if (this.user?.role === 'Admin') {
      this.stats = { totalRequests: 120, pendingRequests: 45, activeRoutes: 8, completedRequests: 67 };
    } else if (this.user?.role === 'Citizen') {
      this.stats = { totalRequests: 12, pendingRequests: 2, activeRoutes: 0, completedRequests: 10 };
    } else {
      this.stats = { totalRequests: 50, pendingRequests: 20, activeRoutes: 1, completedRequests: 29 };
    }
  }
  
  loadActivities() {
    // Mock data
    this.recentActivities = [
      { action: 'Request Created', time: '2 mins ago', details: 'Plastic waste collection request' },
      { action: 'Route Started', time: '1 hour ago', details: 'Route #102 started by Driver Dan' },
      { action: 'User Registered', time: '3 hours ago', details: 'New citizen registration' }
    ];
  }
}
