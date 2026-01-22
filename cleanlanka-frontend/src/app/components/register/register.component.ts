import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

const passwordPolicy: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;

  if (password && confirm && password !== confirm) {
    return { mismatch: true };
  }
  return null;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  error = '';
  success = '';

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$/)
        ]
      ],
      confirmPassword: ['', Validators.required],
      role: ['Citizen', Validators.required],
      zone: ['']
    }, { validators: passwordPolicy });
  }

  get fullName() { return this.registerForm.get('fullName'); }
  get email() { return this.registerForm.get('email'); }
  get password() { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    const { confirmPassword, ...payload } = this.registerForm.value;

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading = false;
        this.success = payload.role === 'Collector'
          ? 'Collector registration submitted. Waiting for admin approval.'
          : 'Account created successfully. You can now sign in.';
        setTimeout(() => {
          this.router.navigate(['/login'], { queryParams: { registered: 'true', email: payload.email } });
        }, 600);
      },
      error: (err) => {
        this.loading = false;
        const errors = err?.error?.errors;
        if (Array.isArray(errors) && errors.length) {
          this.error = errors.join(' ');
        } else {
          this.error = err?.error?.message || 'Registration failed. Please check your details and try again.';
        }
      }
    });
  }
}
