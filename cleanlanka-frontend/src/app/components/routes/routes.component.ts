import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-routes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './routes.component.html',
  styleUrl: './routes.component.scss'
})
export class RoutesComponent implements OnInit {
  http = inject(HttpClient);
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  
  routes: any[] = [];
  vehicles: any[] = [];
  routeForm: FormGroup;
  showModal = false;
  apiUrl = 'http://localhost:5214/api/routes';
  user = this.authService.getUser();

  constructor() {
    this.routeForm = this.fb.group({
      area: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      vehicleId: ['', Validators.required],
      status: ['Scheduled']
    });
  }

  ngOnInit() {
    this.loadRoutes();
    this.loadVehicles();
  }

  loadRoutes() {
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => this.routes = data,
      error: (err) => console.error(err)
    });
  }

  loadVehicles() {
      this.http.get<any[]>('http://localhost:5214/api/vehicles').subscribe({
          next: (data) => this.vehicles = data
      });
  }

  openModal() {
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  submitRoute() {
    if (this.routeForm.invalid) return;
    
    this.http.post(this.apiUrl, this.routeForm.value).subscribe({
      next: () => {
        this.loadRoutes();
        this.closeModal();
        this.routeForm.reset({ status: 'Scheduled' });
      },
      error: (err) => alert('Error creating route')
    });
  }
}
