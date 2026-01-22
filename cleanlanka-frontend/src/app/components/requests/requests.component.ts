import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { API_BASE_URL } from '../../config/api.config';

@Component({
  selector: 'app-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './requests.component.html',
  styleUrl: './requests.component.scss'
})
export class RequestsComponent implements OnInit {
  http = inject(HttpClient);
  authService = inject(AuthService);
  fb = inject(FormBuilder);
  
  requests: any[] = [];
  collectors: any[] = [];
  user = this.authService.getUser();
  showModal = false;
  requestForm: FormGroup;
  
  apiUrl = `${API_BASE_URL}/requests`;

  constructor() {
    this.requestForm = this.fb.group({
      requestType: ['', Validators.required],
      location: ['', Validators.required],
      district: ['', Validators.required],
      municipality: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit() {
    this.loadRequests();
    if (this.canApprove()) {
      this.loadCollectors();
    }
  }

  loadRequests() {
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => this.requests = data,
      error: (err) => console.error('Error loading requests', err)
    });
  }

  openModal() {
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  submitRequest() {
    if (this.requestForm.invalid) return;
    
    this.http.post(this.apiUrl, this.requestForm.value).subscribe({
      next: (res) => {
        this.loadRequests();
        this.closeModal();
        this.requestForm.reset();
      },
      error: (err) => console.error('Error creating request', err)
    });
  }

  updateStatus(id: number, status: string) {
    this.http.put(`${this.apiUrl}/${id}/status`, { status }, { headers: { 'Content-Type': 'application/json' } }).subscribe({
      next: () => this.loadRequests(),
      error: (err) => console.error('Error updating status', err)
    });
  }

  confirmRequest(id: number, collectorId?: string) {
    const status = collectorId ? 'Assigned' : 'Confirmed';
    this.http.put(`${this.apiUrl}/${id}/confirm`, { status, collectorId }, { headers: { 'Content-Type': 'application/json' } }).subscribe({
      next: () => this.loadRequests(),
      error: (err) => console.error('Error confirming request', err)
    });
  }

  loadCollectors() {
    this.http.get<any[]>(`${API_BASE_URL}/users/collectors`).subscribe({
      next: (data) => this.collectors = data,
      error: (err) => console.error('Error loading collectors', err)
    });
  }

  canApprove() {
    return ['Admin', 'MunicipalityOfficer'].includes(this.user.role);
  }

  collectorName(id: string) {
    return this.collectors.find(c => c.id === id)?.fullName || 'Unassigned';
  }
}
