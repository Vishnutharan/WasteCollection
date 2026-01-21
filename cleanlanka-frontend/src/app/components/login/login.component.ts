import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  error = '';
  authService = inject(AuthService);
  router = inject(Router);

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.error = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: (user) => {
        this.loading = false;
        if (user.role === 'Admin') this.router.navigate(['/dashboard']);
        else if (user.role === 'Citizen') this.router.navigate(['/requests']);
        else this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Invalid email or password';
      }
    });
  }

  demoLogin(role: string) {
    let email = '';
    let password = '';
    
    switch(role) {
      case 'Admin': email = 'admin@cleanlanka.com'; password = 'Admin@123'; break;
      case 'Citizen': email = 'citizen@cleanlanka.com'; password = 'Citizen@123'; break;
      case 'Collector': email = 'collector@cleanlanka.com'; password = 'Collector@123'; break;
    }
    
    this.loginForm.setValue({ email, password });
    this.onSubmit();
  }
}
