import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AuthService } from '../auth/auth.service';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';
import { roleLabel as getRoleLabel } from '../../shared/utils/ui-state';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule, MatToolbarModule, HasRoleDirective],
  template: `
    <div class="min-h-screen bg-claims-shell text-claims-ink">
      <mat-toolbar class="!h-16 border-b border-claims-border !bg-claims-surface !px-6">
        <div class="flex w-full items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <div class="grid h-9 w-9 place-items-center rounded bg-claims-navy text-sm font-bold text-white">
              SV
            </div>
            <div>
              <div class="text-base font-semibold leading-tight">SeguraVida Siniestros</div>
              <div class="text-xs text-claims-muted">Gestión de siniestros</div>
            </div>
          </div>

          <div class="flex items-center gap-3 text-sm">
            <span class="rounded border border-claims-border bg-claims-panel px-3 py-1 text-claims-muted">{{ roleLabel() }}</span>
            <button mat-stroked-button type="button" (click)="logout()">
              <mat-icon>logout</mat-icon>
              Cerrar sesión
            </button>
          </div>
        </div>
      </mat-toolbar>

      <div class="grid min-h-[calc(100vh-4rem)] grid-cols-1 lg:grid-cols-[240px_1fr]">
        <aside class="border-b border-claims-border bg-claims-sidebar p-4 lg:border-b-0 lg:border-r">
          <nav class="flex gap-2 lg:flex-col">
            <a
              routerLink="/claims"
              routerLinkActive="!bg-claims-blue !text-white shadow-sm"
              class="flex items-center gap-2 rounded px-3 py-2 text-sm font-medium text-claims-ink"
            >
              <mat-icon>assignment</mat-icon>
              Siniestros
            </a>
            <a
              *appHasRole="'AUDITOR'"
              routerLink="/reports"
              routerLinkActive="!bg-claims-blue !text-white shadow-sm"
              class="flex items-center gap-2 rounded px-3 py-2 text-sm font-medium text-claims-ink"
            >
              <mat-icon>analytics</mat-icon>
              Reportes
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
  readonly roleLabel = computed(() => getRoleLabel(this.auth.session()?.role ?? 'NO_ROLE'));

  logout(): void {
    this.auth.logout();
  }
}
