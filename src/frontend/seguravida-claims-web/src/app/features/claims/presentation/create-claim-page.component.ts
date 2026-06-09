import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { claimTypeLabel as getClaimTypeLabel, errorMessage } from '../../../shared/utils/ui-state';
import { ClaimsApiService } from '../data-access/claims-api.service';
import { ClaimType } from '../domain/claim.models';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { formatDateMat } from '../../../shared/utils/formatDate';

@Component({
  selector: 'app-create-claim-page',
  imports: [RouterLink, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule],
  template: `
    <section class="mx-auto max-w-3xl space-y-4">
      <a routerLink="/claims" class="inline-flex items-center gap-1 text-sm font-medium text-blue-700">
        <mat-icon>arrow_back</mat-icon>
        Volver a siniestros
      </a>

      <div class="rounded border border-slate-200 bg-white p-5">
        <h1 class="text-2xl font-semibold text-slate-950">Registrar siniestro</h1>
        <p class="mt-1 text-sm text-slate-600">Formulario inicial. Las reglas críticas también se validan en backend.</p>

        <form class="mt-5 grid gap-4" [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline">
            <mat-label>Número de póliza</mat-label>
            <input matInput formControlName="policyNumber" placeholder="POL-2026-AUTO-001" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Tipo</mat-label>
            <mat-select formControlName="type">
              @for (type of types; track type) {
                <mat-option [value]="type">{{ claimTypeLabel(type) }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <div class="grid gap-4 md:grid-cols-2">
            <mat-form-field appearance="outline">
              <mat-label>Fecha del incidente</mat-label>
              <input matInput [matDatepicker]="fromDatePicker" formControlName="incidentDate" />
              <mat-datepicker-toggle matSuffix [for]="fromDatePicker"></mat-datepicker-toggle>
              <mat-datepicker #fromDatePicker></mat-datepicker>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Fecha de reporte</mat-label>
              <input matInput [matDatepicker]="toDatePicker" formControlName="reportedDate" />
              <mat-datepicker-toggle matSuffix [for]="toDatePicker"></mat-datepicker-toggle>
              <mat-datepicker #toDatePicker></mat-datepicker>
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>Monto reclamado</mat-label>
            <input matInput type="number" formControlName="claimedAmount" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Descripción</mat-label>
            <textarea matInput rows="4" formControlName="description"></textarea>
          </mat-form-field>

          @if (error()) {
            <div class="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">{{ error() }}</div>
          }

          <div class="flex justify-end gap-2">
            <a mat-stroked-button routerLink="/claims">Cancelar</a>
            <button mat-flat-button type="submit" [disabled]="loading()">Crear</button>
          </div>
        </form>
      </div>
    </section>
  `,
})
export class CreateClaimPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ClaimsApiService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly claimTypeLabel = getClaimTypeLabel;
  readonly types: ClaimType[] = ['ACCIDENT', 'THEFT', 'MEDICAL', 'DEATH', 'PROPERTY_DAMAGE'];
  readonly form = this.fb.nonNullable.group({
    policyNumber: ['', [Validators.required]],
    type: ['ACCIDENT' as ClaimType, [Validators.required]],
    incidentDate: ['', [Validators.required]],
    reportedDate: ['', [Validators.required]],
    claimedAmount: [0, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const incidentDate = formatDateMat(value.incidentDate);
    const reportedDate = formatDateMat(value.reportedDate);

    if (incidentDate > reportedDate) {
      this.error.set('La fecha del incidente no puede ser posterior a la fecha de reporte.');
      return;
    }

    const payload = {
      ...value,
      incidentDate,
      reportedDate,
    };

    this.loading.set(true);
    this.error.set(null);
    this.api
      .createClaim(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => void this.router.navigate(['/claims', response.claimId]),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }
}
