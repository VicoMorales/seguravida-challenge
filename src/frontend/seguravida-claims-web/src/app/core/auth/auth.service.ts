import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { AuthSession, LoginResponse, UserRole } from './auth.models';

const STORAGE_KEY = 'seguravida.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly sessionState = signal<AuthSession | null>(this.readSession());

  readonly session = this.sessionState.asReadonly();
  readonly isAuthenticated = computed(() => this.sessionState() !== null);
  readonly role = computed(() => this.sessionState()?.role ?? null);

  login(role: UserRole) {
    const email = this.emailForRole(role);

    return this.http.post<LoginResponse>(`${this.apiBaseUrl}/auth/login`, { email }).pipe(
      tap((response) => {
        const session: AuthSession = {
          accessToken: response.accessToken,
          email: response.email,
          role: response.role,
          displayName: response.displayName,
        };

        localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        this.sessionState.set(session);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.sessionState.set(null);
    void this.router.navigateByUrl('/login');
  }

  hasAnyRole(roles: UserRole[]): boolean {
    const role = this.role();
    return role !== null && roles.includes(role);
  }

  token(): string | null {
    return this.sessionState()?.accessToken ?? null;
  }

  private emailForRole(role: UserRole): string {
    return {
      OPERATOR: 'operator@seguravida.com',
      ADJUSTER: 'adjuster@seguravida.com',
      AUDITOR: 'auditor@seguravida.com',
    }[role];
  }

  private readSession(): AuthSession | null {
    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthSession;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
