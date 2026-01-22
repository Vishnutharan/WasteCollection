import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { API_BASE_URL } from '../../config/api.config';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.scss'
})
export class VehiclesComponent implements OnInit {
  http = inject(HttpClient);
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  
  vehicles: any[] = [];
  collectors: any[] = [];
  vehicleForm: FormGroup;
  showModal = false;
  apiUrl = `${API_BASE_URL}/vehicles`;
  user = this.authService.getUser();

  constructor() {
    this.vehicleForm = this.fb.group({
      numberPlate: ['', Validators.required],
      type: ['Truck', Validators.required],
      capacity: [1000, Validators.required],
      driverId: ['']
    });
  }

  ngOnInit() {
    this.loadVehicles();
    this.loadCollectors();
  }

  loadVehicles() {
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => this.vehicles = data,
      error: (err) => console.error(err)
    });
  }

  openModal() {
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  submitVehicle() {
    if (this.vehicleForm.invalid) return;
    
    this.http.post(this.apiUrl, this.vehicleForm.value).subscribe({
      next: () => {
        this.loadVehicles();
        this.closeModal();
        this.vehicleForm.reset({ type: 'Truck', capacity: 1000, driverId: '' });
      },
      error: (err) => alert('Error adding vehicle')
    });
  }

  loadCollectors() {
    this.http.get<any[]>(`${API_BASE_URL}/users/collectors`).subscribe({
      next: (data) => this.collectors = data,
      error: (err) => console.error('Error loading collectors', err)
    });
  }

  collectorName(id: string) {
    return this.collectors.find(c => c.id === id)?.fullName || '';
  }

  assignCollector(vehicle: any) {
    if (!vehicle.driverId) return;
    this.http.put(`${this.apiUrl}/${vehicle.id}/assign`, { collectorId: vehicle.driverId }, { headers: { 'Content-Type': 'application/json' } }).subscribe({
      next: () => this.loadVehicles(),
      error: (err) => alert('Error assigning collector')
    });
  }
}
