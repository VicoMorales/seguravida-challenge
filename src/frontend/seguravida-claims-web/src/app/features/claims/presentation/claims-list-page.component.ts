import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';

import { HasRoleDirective } from '../../../shared/directives/has-role.directive';
import { statusClass } from '../../../shared/utils/ui-state';
import { ClaimsFacade } from '../application/claims.facade';
import { ClaimBranch, ClaimStatus } from '../domain/claim.models';

@Component({
  selector: 'app-claims-list-page',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
    HasRoleDirective,
  ],
  providers: [ClaimsFacade],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col justify-between gap-3 md:flex-row md:items-center">
        <div>
          <h1 class="text-2xl font-semibold text-slate-950">Claims</h1>
          <p class="text-sm text-slate-600">Listado operativo con filtros, busqueda y paginacion.</p>
        </div>

        <a *appHasRole="'OPERATOR'" mat-flat-button routerLink="/claims/new">
          <mat-icon>add</mat-icon>
          Register Claim
        </a>
      </header>

      <section class="rounded border border-slate-200 bg-white p-4">
        <div class="grid gap-3 md:grid-cols-[1fr_180px_180px]">
          <mat-form-field appearance="outline">
            <mat-label>Search</mat-label>
            <input matInput [formControl]="search" placeholder="Claim or policy number" (keyup.enter)="applyFilters()" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select [formControl]="status" (selectionChange)="applyFilters()">
              <mat-option value="">All</mat-option>
              @for (item of statuses; track item) {
                <mat-option [value]="item">{{ item }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Branch</mat-label>
            <mat-select [formControl]="branch" (selectionChange)="applyFilters()">
              <mat-option value="">All</mat-option>
              @for (item of branches; track item) {
                <mat-option [value]="item">{{ item }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>
      </section>

      <section class="overflow-hidden rounded border border-slate-200 bg-white">
        @if (facade.loading()) {
          <div class="grid min-h-64 place-items-center">
            <mat-spinner diameter="36" />
          </div>
        } @else if (facade.error()) {
          <div class="p-6 text-sm text-red-700">{{ facade.error() }}</div>
        } @else if (facade.claims().length === 0) {
          <div class="p-6 text-sm text-slate-600">No claims found.</div>
        } @else {
          <table mat-table [dataSource]="facade.claims()" class="w-full">
            <ng-container matColumnDef="claimNumber">
              <th mat-header-cell *matHeaderCellDef>Claim</th>
              <td mat-cell *matCellDef="let claim">
                <a class="font-semibold text-blue-700" [routerLink]="['/claims', claim.claimId]">{{ claim.claimNumber }}</a>
              </td>
            </ng-container>

            <ng-container matColumnDef="policyNumber">
              <th mat-header-cell *matHeaderCellDef>Policy</th>
              <td mat-cell *matCellDef="let claim">{{ claim.policyNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="branch">
              <th mat-header-cell *matHeaderCellDef>Branch</th>
              <td mat-cell *matCellDef="let claim">{{ claim.branch }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let claim">
                <span class="status-badge" [class]="statusClass(claim.status)">{{ claim.status }}</span>
              </td>
            </ng-container>

            <ng-container matColumnDef="reportedDate">
              <th mat-header-cell *matHeaderCellDef>Reported</th>
              <td mat-cell *matCellDef="let claim">{{ claim.reportedDate }}</td>
            </ng-container>

            <ng-container matColumnDef="claimedAmount">
              <th mat-header-cell *matHeaderCellDef>Claimed</th>
              <td mat-cell *matCellDef="let claim">{{ claim.claimedAmount | currency }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
        }

        <mat-paginator
          [length]="facade.totalCount()"
          [pageSize]="facade.filters().pageSize"
          [pageIndex]="facade.filters().page - 1"
          [pageSizeOptions]="[5, 10, 20]"
          (page)="page($event)"
        />
      </section>
    </section>
  `,
})
export class ClaimsListPageComponent implements OnInit {
  readonly facade = inject(ClaimsFacade);
  readonly search = new FormControl('', { nonNullable: true });
  readonly status = new FormControl<ClaimStatus | ''>('', { nonNullable: true });
  readonly branch = new FormControl<ClaimBranch | ''>('', { nonNullable: true });
  readonly statuses: ClaimStatus[] = ['REPORTED', 'UNDER_REVIEW', 'APPROVED', 'REJECTED', 'PAID'];
  readonly branches: ClaimBranch[] = ['AUTO', 'LIFE', 'HEALTH', 'HOME'];
  readonly columns = ['claimNumber', 'policyNumber', 'branch', 'status', 'reportedDate', 'claimedAmount'];
  readonly statusClass = statusClass;

  ngOnInit(): void {
    this.facade.load();
  }

  applyFilters(): void {
    this.facade.patchFilters({
      search: this.search.value.trim(),
      status: this.status.value,
      branch: this.branch.value,
    });
  }

  page(event: PageEvent): void {
    this.facade.patchFilters({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
