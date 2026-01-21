import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { MainLayoutComponent } from './components/layout/main-layout/main-layout.component';
import { UsersComponent } from './components/users/users.component';
import { RequestsComponent } from './components/requests/requests.component';
import { VehiclesComponent } from './components/vehicles/vehicles.component';
import { RoutesComponent } from './components/routes/routes.component';
import { NotificationsComponent } from './components/notifications/notifications.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'users', component: UsersComponent },
      { path: 'requests', component: RequestsComponent },
      { path: 'vehicles', component: VehiclesComponent },
      { path: 'routes', component: RoutesComponent },
      { path: 'notifications', component: NotificationsComponent },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
