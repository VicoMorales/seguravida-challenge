import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AuthService } from '../auth/auth.service';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule, MatToolbarModule, HasRoleDirective],
  template: `
    <div class="min-h-screen bg-slate-100 text-slate-950">
      <mat-toolbar class="!h-16 border-b border-slate-200 !bg-white !px-6">
        <div class="flex w-full items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <div class="grid h-9 w-9 place-items-center rounded bg-blue-700 text-sm font-bold text-white">
              SV
            </div>
            <div>
              <div class="text-base font-semibold leading-tight">SeguraVida Claims</div>
              <div class="text-xs text-slate-500">Gestion de siniestros</div>
            </div>
          </div>

          <div class="flex items-center gap-3 text-sm">
            <span class="rounded border border-slate-200 px-3 py-1 text-slate-600">{{ roleLabel() }}</span>
            <button mat-stroked-button type="button" (click)="logout()">
              <mat-icon>logout</mat-icon>
              Logout
            </button>
          </div>
        </div>
      </mat-toolbar>

      <div class="grid min-h-[calc(100vh-4rem)] grid-cols-1 lg:grid-cols-[240px_1fr]">
        <aside class="border-b border-slate-200 bg-white p-4 lg:border-b-0 lg:border-r">
          <nav class="flex gap-2 lg:flex-col">
            <a
              routerLink="/claims"
              routerLinkActive="bg-blue-70 text-blue-700"
              class="flex items-center gap-2 rounded px-3 py-2 text-sm font-medium text-slate-700"
            >
              <mat-icon>assignment</mat-icon>
              Claims
            </a>
            <a
              *appHasRole="'AUDITOR'"
              routerLink="/reports"
              routerLinkActive="bg-blue-70 text-blue-700"
              class="flex items-center gap-2 rounded px-3 py-2 text-sm font-medium text-slate-700"
            >
              <mat-icon>analytics</mat-icon>
              Reports
            </a>
          </nav>
        </aside>

        <main class="min-w-0 p-4 md:p-6">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class AppLayoutComponent {
  private readonly auth = inject(AuthService);
  readonly roleLabel = computed(() => this.auth.session()?.role ?? 'NO_ROLE');

  logout(): void {
    this.auth.logout();
  }
}
