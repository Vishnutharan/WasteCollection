import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './requests.component.html',
  styleUrl: './requests.component.scss'
})
export class RequestsComponent implements OnInit {
  http = inject(HttpClient);
  authService = inject(AuthService);
  fb = inject(FormBuilder);
  
  requests: any[] = [];
  user = this.authService.getUser();
  showModal = false;
  requestForm: FormGroup;
  
  apiUrl = 'http://localhost:5214/api/requests';

  constructor() {
    this.requestForm = this.fb.group({
      requestType: ['Plastic', Validators.required],
      location: ['', Validators.required],
      district: ['', Validators.required],
      municipality: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit() {
    this.loadRequests();
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
        this.requestForm.reset({ requestType: 'Plastic' });
      },
      error: (err) => console.error('Error creating request', err)
    });
  }

  updateStatus(id: number, status: string) {
    this.http.put(`${this.apiUrl}/${id}/status`, `"${status}"`, { headers: { 'Content-Type': 'application/json' } }).subscribe({
      next: () => this.loadRequests()
    });
  }

  canApprove() {
    return ['Admin', 'MunicipalityOfficer'].includes(this.user.role);
  }
}
