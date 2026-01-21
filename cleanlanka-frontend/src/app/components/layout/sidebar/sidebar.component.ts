import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  authService = inject(AuthService);
  router = inject(Router);
  user = this.authService.getUser();

  menuItems = [
    { label: 'Dashboard', icon: 'fas fa-chart-line', route: '/dashboard', roles: ['Admin', 'MunicipalityOfficer', 'Collector', 'Citizen'] },
    { label: 'User Management', icon: 'fas fa-users', route: '/users', roles: ['Admin'] },
    { label: 'Waste Requests', icon: 'fas fa-dumpster', route: '/requests', roles: ['Admin', 'MunicipalityOfficer', 'Collector', 'Citizen'] },
    { label: 'Routes & Schedules', icon: 'fas fa-route', route: '/routes', roles: ['Admin', 'MunicipalityOfficer', 'Collector'] },
    { label: 'Vehicles', icon: 'fas fa-truck', route: '/vehicles', roles: ['Admin', 'MunicipalityOfficer'] },
    { label: 'Notifications', icon: 'fas fa-bell', route: '/notifications', roles: ['Admin', 'MunicipalityOfficer', 'Collector', 'Citizen'] }
  ];

  logout() {
    this.authService.logout();
  }

  hasRole(allowedRoles: string[]): boolean {
    return this.user && allowedRoles.includes(this.user.role);
  }
}
