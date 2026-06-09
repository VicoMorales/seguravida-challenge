import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, debounceTime, distinctUntilChanged, EMPTY, finalize, switchMap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import {
  branchLabel as getBranchLabel,
  claimTypeLabel as getClaimTypeLabel,
  errorMessage,
} from '../../../shared/utils/ui-state';
import { ClaimsApiService } from '../data-access/claims-api.service';
import { ClaimType, PolicyLookup } from '../domain/claim.models';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { formatDateMat } from '../../../shared/utils/formatDate';

@Component({
  selector: 'app-create-claim-page',
  imports: [
    CurrencyPipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
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

          @if (policyLoading()) {
            <div class="flex items-center gap-2 rounded border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
              <mat-spinner diameter="20" />
              Consultando poliza...
            </div>
          } @else if (policyLookup()) {
            <section class="grid gap-3 rounded border border-slate-200 bg-slate-50 p-4 md:grid-cols-2">
              <div>
                <h2 class="text-sm font-semibold text-slate-900">Poliza</h2>
                <dl class="mt-3 grid gap-2 text-sm">
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Ramo</dt><dd class="font-medium text-slate-900">{{ branchLabel(policyLookup()!.policy.branch) }}</dd></div>
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Vigencia</dt><dd class="font-medium text-slate-900">{{ policyLookup()!.policy.startDate }} - {{ policyLookup()!.policy.endDate }}</dd></div>
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Suma asegurada</dt><dd class="font-medium text-slate-900">{{ policyLookup()!.policy.insuredAmount | currency }}</dd></div>
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Estado</dt><dd class="font-medium text-slate-900">{{ policyStatusLabel(policyLookup()!.policy.status) }}</dd></div>
                </dl>
              </div>
              <div>
                <h2 class="text-sm font-semibold text-slate-900">Asegurado</h2>
                <dl class="mt-3 grid gap-2 text-sm">
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Nombre</dt><dd class="font-medium text-slate-900">{{ policyLookup()!.insuredParty.fullName }}</dd></div>
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Documento</dt><dd class="font-medium text-slate-900">{{ policyLookup()!.insuredParty.maskedDocumentId }}</dd></div>
                  <div class="flex justify-between gap-3"><dt class="text-slate-500">Email</dt><dd class="font-medium text-slate-900">{{ policyLookup()!.insuredParty.maskedEmail }}</dd></div>
                </dl>
              </div>
            </section>
          } @else if (policyLookupError()) {
            <div class="rounded border border-amber-200 bg-amber-50 p-3 text-sm text-amber-700">{{ policyLookupError() }}</div>
          }

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
  readonly policyLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly policyLookupError = signal<string | null>(null);
  readonly policyLookup = signal<PolicyLookup | null>(null);
  readonly claimTypeLabel = getClaimTypeLabel;
  readonly branchLabel = getBranchLabel;
  readonly types: ClaimType[] = ['ACCIDENT', 'THEFT', 'MEDICAL', 'DEATH', 'PROPERTY_DAMAGE'];
  readonly form = this.fb.nonNullable.group({
    policyNumber: ['', [Validators.required]],
    type: ['ACCIDENT' as ClaimType, [Validators.required]],
    incidentDate: ['', [Validators.required]],
    reportedDate: ['', [Validators.required]],
    claimedAmount: [0, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  constructor() {
    this.form.controls.policyNumber.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap((policyNumber) => {
          this.policyLookup.set(null);
          this.policyLookupError.set(null);

          const trimmedPolicyNumber = policyNumber.trim();

          if (trimmedPolicyNumber.length < 3) {
            this.policyLoading.set(false);
            return EMPTY;
          }

          this.policyLoading.set(true);

          return this.api.getPolicy(trimmedPolicyNumber).pipe(
            finalize(() => this.policyLoading.set(false)),
            catchError((error) => {
              this.policyLookupError.set(errorMessage(error));
              return EMPTY;
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((policyLookup) => this.policyLookup.set(policyLookup));
  }

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

  policyStatusLabel(status: string): string {
    return {
      ACTIVE: 'Activa',
      EXPIRED: 'Vencida',
      CANCELLED: 'Cancelada',
    }[status] ?? status;
  }
}
