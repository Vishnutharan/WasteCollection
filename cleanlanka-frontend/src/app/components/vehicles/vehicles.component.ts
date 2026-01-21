import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.scss'
})
export class VehiclesComponent implements OnInit {
  http = inject(HttpClient);
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  
  vehicles: any[] = [];
  vehicleForm: FormGroup;
  showModal = false;
  apiUrl = 'http://localhost:5214/api/vehicles';
  user = this.authService.getUser();

  constructor() {
    this.vehicleForm = this.fb.group({
      numberPlate: ['', Validators.required],
      type: ['Truck', Validators.required],
      capacity: [1000, Validators.required]
    });
  }

  ngOnInit() {
    this.loadVehicles();
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
        this.vehicleForm.reset({ type: 'Truck', capacity: 1000 });
      },
      error: (err) => alert('Error adding vehicle')
    });
  }
}
