import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { UserRole } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';
import { errorMessage } from '../../../shared/utils/ui-state';

@Component({
  selector: 'app-login-page',
  imports: [MatButtonModule, MatCardModule, MatIconModule],
  template: `
    <main class="grid min-h-screen place-items-center bg-slate-100 p-4">
      <section class="w-full max-w-md">
        <div class="mb-6">
          <div class="mb-3 grid h-12 w-12 place-items-center rounded bg-blue-700 font-bold text-white">SV</div>
          <h1 class="text-2xl font-semibold text-slate-950">SeguraVida Claims</h1>
          <p class="mt-1 text-sm text-slate-600">Selecciona un rol mock para continuar.</p>
        </div>

        <mat-card class="!rounded !border !border-slate-200 !shadow-sm">
          <mat-card-content class="!p-4">
            <div class="grid gap-3">
              @for (role of roles; track role.value) {
                <button
                  mat-stroked-button
                  type="button"
                  class="!h-auto !justify-start !py-3"
                  [disabled]="loading()"
                  (click)="login(role.value)"
                >
                  <mat-icon>{{ role.icon }}</mat-icon>
                  <span class="ml-2 text-left">
                    <span class="block font-semibold">{{ role.value }}</span>
                    <span class="block text-xs text-slate-500">{{ role.description }}</span>
                  </span>
                </button>
              }
            </div>

            @if (error()) {
              <div class="mt-4 rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                {{ error() }}
              </div>
            }
          </mat-card-content>
        </mat-card>
      </section>
    </main>
  `,
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly roles: { value: UserRole; icon: string; description: string }[] = [
    { value: 'OPERATOR', icon: 'edit_note', description: 'Registra y consulta siniestros' },
    { value: 'ADJUSTER', icon: 'fact_check', description: 'Peritaje, aprobacion y pagos' },
    { value: 'AUDITOR', icon: 'visibility', description: 'Consulta, historial y reportes' },
  ];

  login(role: UserRole): void {
    this.loading.set(true);
    this.error.set(null);

    this.auth.login(role).subscribe({
      next: () => void this.router.navigateByUrl('/claims'),
      error: (error) => {
        this.error.set(errorMessage(error));
        this.loading.set(false);
      },
    });
  }
}
