import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  error = '';
  success = '';
  returnUrl = '';
  authService = inject(AuthService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const registered = params.get('registered');
      const email = params.get('email');
      this.returnUrl = params.get('returnUrl') || '';

      if (registered) {
        this.success = 'Registration successful. Please sign in.';
      }
      if (email) {
        this.loginForm.patchValue({ email });
      }
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: (user) => {
        this.loading = false;
        const fallbackRoute = user.role === 'Citizen' ? '/requests' : '/dashboard';
        const target = this.returnUrl || fallbackRoute;
        this.router.navigate([target]);
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
