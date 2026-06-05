import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppLayoutComponent } from './core/layout/app-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/presentation/login-page.component').then((component) => component.LoginPageComponent),
  },
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'claims',
        loadComponent: () =>
          import('./features/claims/presentation/claims-list-page.component').then((component) => component.ClaimsListPageComponent),
      },
      {
        path: 'claims/new',
        canActivate: [roleGuard(['OPERATOR'])],
        loadComponent: () =>
          import('./features/claims/presentation/create-claim-page.component').then((component) => component.CreateClaimPageComponent),
      },
      {
        path: 'claims/:id',
        loadComponent: () =>
          import('./features/claims/presentation/claim-detail-page.component').then((component) => component.ClaimDetailPageComponent),
      },
      {
        path: 'reports',
        canActivate: [roleGuard(['AUDITOR'])],
        loadComponent: () =>
          import('./features/reports/presentation/reports-page.component').then((component) => component.ReportsPageComponent),
      },
      { path: '', pathMatch: 'full', redirectTo: 'claims' },
    ],
  },
  { path: '**', redirectTo: 'claims' },
];
