import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { errorMessage } from '../../../shared/utils/ui-state';
import { ClaimsApiService } from '../data-access/claims-api.service';
import { ClaimType } from '../domain/claim.models';

@Component({
  selector: 'app-create-claim-page',
  imports: [RouterLink, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule],
  template: `
    <section class="mx-auto max-w-3xl space-y-4">
      <a routerLink="/claims" class="inline-flex items-center gap-1 text-sm font-medium text-blue-700">
        <mat-icon>arrow_back</mat-icon>
        Back to claims
      </a>

      <div class="rounded border border-slate-200 bg-white p-5">
        <h1 class="text-2xl font-semibold text-slate-950">Register Claim</h1>
        <p class="mt-1 text-sm text-slate-600">Formulario inicial. Las reglas criticas tambien se validan en backend.</p>

        <form class="mt-5 grid gap-4" [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline">
            <mat-label>Policy id</mat-label>
            <input matInput formControlName="policyId" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Type</mat-label>
            <mat-select formControlName="type">
              @for (type of types; track type) {
                <mat-option [value]="type">{{ type }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <div class="grid gap-4 md:grid-cols-2">
            <mat-form-field appearance="outline">
              <mat-label>Incident date</mat-label>
              <input matInput type="date" formControlName="incidentDate" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Reported date</mat-label>
              <input matInput type="date" formControlName="reportedDate" />
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>Claimed amount</mat-label>
            <input matInput type="number" formControlName="claimedAmount" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Description</mat-label>
            <textarea matInput rows="4" formControlName="description"></textarea>
          </mat-form-field>

          @if (error()) {
            <div class="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">{{ error() }}</div>
          }

          <div class="flex justify-end gap-2">
            <a mat-stroked-button routerLink="/claims">Cancel</a>
            <button mat-flat-button type="submit" [disabled]="loading()">Create</button>
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
  readonly types: ClaimType[] = ['ACCIDENT', 'THEFT', 'MEDICAL', 'DEATH', 'PROPERTY_DAMAGE'];
  readonly form = this.fb.nonNullable.group({
    policyId: ['', [Validators.required]],
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

    if (value.incidentDate > value.reportedDate) {
      this.error.set('Incident date cannot be after reported date.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.api
      .createClaim(value)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => void this.router.navigate(['/claims', response.claimId]),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }
}
