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
import { branchLabel as getBranchLabel, statusClass, statusLabel as getStatusLabel } from '../../../shared/utils/ui-state';
import { ClaimsFacade } from '../application/claims.facade';
import { ClaimBranch, ClaimStatus } from '../domain/claim.models';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { formatDateMat } from '../../../shared/utils/formatDate';

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
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    HasRoleDirective,
  ],
  providers: [ClaimsFacade],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col justify-between gap-3 md:flex-row md:items-center">
        <div>
          <h1 class="text-2xl font-semibold text-claims-ink">Siniestros</h1>
          <p class="text-sm text-claims-muted">Listado operativo con filtros, búsqueda y paginación.</p>
        </div>

        <a *appHasRole="'OPERATOR'" mat-flat-button routerLink="/claims/new">
          <mat-icon>add</mat-icon>
          Registrar siniestro
        </a>
      </header>

      <section class="rounded border border-claims-border bg-claims-surface p-4">
        <div class="grid gap-3 md:grid-cols-[1fr_160px_160px_160px_160px]">
          <mat-form-field appearance="outline">
            <mat-label>Buscar</mat-label>
            <input matInput [formControl]="search" placeholder="Siniestro, póliza o documento" (keyup.enter)="applyFilters()" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Estado</mat-label>
            <mat-select [formControl]="status" (selectionChange)="applyFilters()">
              <mat-option value="">Todos</mat-option>
              @for (item of statuses; track item) {
                <mat-option [value]="item">{{ statusLabel(item) }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Ramo</mat-label>
            <mat-select [formControl]="branch" (selectionChange)="applyFilters()">
              <mat-option value="">Todos</mat-option>
              @for (item of branches; track item) {
                <mat-option [value]="item">{{ branchLabel(item) }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Desde</mat-label>
            <input matInput [matDatepicker]="fromDatePicker" [formControl]="fromDate" (dateChange)="applyFilters()" />
            <mat-datepicker-toggle matSuffix [for]="fromDatePicker"></mat-datepicker-toggle>
            <mat-datepicker #fromDatePicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Hasta</mat-label>
            <input matInput [matDatepicker]="toDatePicker" [formControl]="toDate" (dateChange)="applyFilters()" />
            <mat-datepicker-toggle matSuffix [for]="toDatePicker"></mat-datepicker-toggle>
            <mat-datepicker #toDatePicker></mat-datepicker>
          </mat-form-field>
        </div>
      </section>

      <section class="overflow-hidden rounded border border-claims-border bg-claims-surface">
        @if (facade.loading()) {
          <div class="grid min-h-64 place-items-center">
            <mat-spinner diameter="36" />
          </div>
        } @else if (facade.error()) {
          <div class="p-6 text-sm text-claims-danger">{{ facade.error() }}</div>
        } @else if (facade.claims().length === 0) {
          <div class="p-6 text-sm text-claims-muted">No se encontraron siniestros.</div>
        } @else {
          <table mat-table [dataSource]="facade.claims()" class="w-full">
            <ng-container matColumnDef="claimNumber">
              <th mat-header-cell *matHeaderCellDef>Siniestro</th>
              <td mat-cell *matCellDef="let claim">
                <a class="font-semibold text-claims-blue" [routerLink]="['/claims', claim.claimId]">{{ claim.claimNumber }}</a>
              </td>
            </ng-container>

            <ng-container matColumnDef="policyNumber">
              <th mat-header-cell *matHeaderCellDef>Póliza</th>
              <td mat-cell *matCellDef="let claim">{{ claim.policyNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="branch">
              <th mat-header-cell *matHeaderCellDef>Ramo</th>
              <td mat-cell *matCellDef="let claim">{{ branchLabel(claim.branch) }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let claim">
                <span class="status-badge" [class]="statusClass(claim.status)">{{ statusLabel(claim.status) }}</span>
              </td>
            </ng-container>

            <ng-container matColumnDef="reportedDate">
              <th mat-header-cell *matHeaderCellDef>Reportado</th>
              <td mat-cell *matCellDef="let claim">{{ claim.reportedDate }}</td>
            </ng-container>

            <ng-container matColumnDef="claimedAmount">
              <th mat-header-cell *matHeaderCellDef>Monto reclamado</th>
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
  readonly fromDate = new FormControl('', { nonNullable: true });
  readonly toDate = new FormControl('', { nonNullable: true });
  readonly statuses: ClaimStatus[] = ['REPORTED', 'UNDER_REVIEW', 'APPROVED', 'REJECTED', 'PAID'];
  readonly branches: ClaimBranch[] = ['AUTO', 'LIFE', 'HEALTH', 'HOME'];
  readonly columns = ['claimNumber', 'policyNumber', 'branch', 'status', 'reportedDate', 'claimedAmount'];
  readonly statusClass = statusClass;
  readonly statusLabel = getStatusLabel;
  readonly branchLabel = getBranchLabel;

  ngOnInit(): void {
    this.facade.load();
  }

  applyFilters(): void {
    this.facade.patchFilters({
      search: this.search.value.trim(),
      status: this.status.value,
      branch: this.branch.value,
      fromDate: formatDateMat(this.fromDate.value),
      toDate: formatDateMat(this.toDate.value),
    });
  }

  page(event: PageEvent): void {
    this.facade.patchFilters({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
