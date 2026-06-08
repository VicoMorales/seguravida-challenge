import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';

import { branchLabel as getBranchLabel, errorMessage, statusLabel as getStatusLabel } from '../../../shared/utils/ui-state';
import { ReportsApiService } from '../data-access/reports-api.service';
import { ClaimsSummaryRow } from '../domain/report.models';

@Component({
  selector: 'app-reports-page',
  imports: [CurrencyPipe, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatTableModule],
  template: `
    <section class="space-y-4">
      <header>
        <h1 class="text-2xl font-semibold text-slate-950">Resumen de siniestros</h1>
        <p class="text-sm text-slate-600">Totales por ramo y estado con monto pagado.</p>
      </header>

      <section class="rounded border border-slate-200 bg-white p-4">
        <form class="grid gap-3 md:grid-cols-[180px_180px_auto]" [formGroup]="form" (ngSubmit)="load()">
          <mat-form-field appearance="outline">
            <mat-label>Fecha desde</mat-label>
            <input matInput type="date" formControlName="fromDate" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Fecha hasta</mat-label>
            <input matInput type="date" formControlName="toDate" />
          </mat-form-field>
          <button mat-flat-button type="submit" class="!h-14">Aplicar</button>
        </form>
      </section>

      <section class="overflow-hidden rounded border border-slate-200 bg-white">
        @if (loading()) {
          <div class="grid min-h-56 place-items-center"><mat-spinner diameter="36" /></div>
        } @else if (error()) {
          <div class="p-6 text-sm text-red-700">{{ error() }}</div>
        } @else {
          <table mat-table [dataSource]="rows()" class="w-full">
            <ng-container matColumnDef="branch">
              <th mat-header-cell *matHeaderCellDef>Ramo</th>
              <td mat-cell *matCellDef="let row">{{ branchLabel(row.branch) }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let row">{{ statusLabel(row.status) }}</td>
            </ng-container>
            <ng-container matColumnDef="totalClaims">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let row">{{ row.totalClaims }}</td>
            </ng-container>
            <ng-container matColumnDef="paidAmount">
              <th mat-header-cell *matHeaderCellDef>Monto pagado</th>
              <td mat-cell *matCellDef="let row">{{ row.paidAmount | currency }}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
        }
      </section>
    </section>
  `,
})
export class ReportsPageComponent implements OnInit {
  private readonly api = inject(ReportsApiService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly rows = signal<ClaimsSummaryRow[]>([]);
  readonly columns = ['branch', 'status', 'totalClaims', 'paidAmount'];
  readonly statusLabel = getStatusLabel;
  readonly branchLabel = getBranchLabel;
  readonly form = this.fb.nonNullable.group({
    fromDate: [''],
    toDate: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const { fromDate, toDate } = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);
    this.api
      .getClaimsSummary(fromDate, toDate)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.rows.set(rows),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }
}
