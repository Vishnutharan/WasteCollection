import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  http = inject(HttpClient);
  fb = inject(FormBuilder);
  
  users: any[] = [];
  userForm: FormGroup;
  showModal = false;
  apiUrl = 'http://localhost:5214/api/users';

  constructor() {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      fullName: ['', Validators.required],
      role: ['Citizen', Validators.required],
      zone: ['']
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => this.users = data,
      error: (err) => console.error(err)
    });
  }

  openModal() {
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  submitUser() {
    if (this.userForm.invalid) return;
    
    // Use auth/register or users create endpoint
    this.http.post(this.apiUrl, this.userForm.value).subscribe({
      next: () => {
        this.loadUsers();
        this.closeModal();
        this.userForm.reset({ role: 'Citizen' });
      },
      error: (err) => alert('Error creating user')
    });
  }

  deleteUser(id: string) {
    if(confirm('Are you sure?')) {
      this.http.delete(`${this.apiUrl}/${id}`).subscribe(() => this.loadUsers());
    }
  }
}
