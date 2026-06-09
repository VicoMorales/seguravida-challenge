import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { HasRoleDirective } from '../../../shared/directives/has-role.directive';
import {
  branchLabel as getBranchLabel,
  claimTypeLabel as getClaimTypeLabel,
  errorMessage,
  statusClass,
  statusLabel as getStatusLabel,
} from '../../../shared/utils/ui-state';
import { ClaimsApiService } from '../data-access/claims-api.service';
import { ClaimDetail } from '../domain/claim.models';

@Component({
  selector: 'app-claim-detail-page',
  imports: [
    CurrencyPipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    HasRoleDirective,
  ],
  template: `
    <section class="space-y-4">
      <a routerLink="/claims" class="inline-flex items-center gap-1 text-sm font-medium text-claims-blue">
        <mat-icon>arrow_back</mat-icon>
        Volver a siniestros
      </a>

      @if (loading()) {
        <div class="grid min-h-64 place-items-center"><mat-spinner diameter="36" /></div>
      } @else if (error()) {
        <div class="rounded border border-red-200 bg-claims-danger-soft p-4 text-sm text-claims-danger">{{ error() }}</div>
      } @else if (claim()) {
        <header class="rounded border border-claims-border bg-claims-surface p-5">
          <div class="flex flex-col justify-between gap-3 md:flex-row md:items-start">
            <div>
              <h1 class="text-2xl font-semibold text-claims-ink">{{ claim()!.claimNumber }}</h1>
              <p class="mt-1 text-sm text-claims-muted">{{ claim()!.description }}</p>
            </div>
            <span class="status-badge" [class]="statusClass(claim()!.status)">{{ statusLabel(claim()!.status) }}</span>
          </div>
        </header>

        <section class="grid gap-4 xl:grid-cols-[1fr_420px]">
          <div class="space-y-4">
            <div class="rounded border border-claims-border bg-claims-surface p-5">
              <h2 class="mb-4 text-base font-semibold">Datos del siniestro</h2>
              <dl class="grid gap-4 md:grid-cols-2">
                <div><dt class="text-xs text-claims-muted">Póliza</dt><dd class="font-medium">{{ claim()!.policyNumber }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Ramo</dt><dd class="font-medium">{{ branchLabel(claim()!.branch) }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Tipo</dt><dd class="font-medium">{{ claimTypeLabel(claim()!.type) }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Fecha del incidente</dt><dd class="font-medium">{{ claim()!.incidentDate }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Fecha de reporte</dt><dd class="font-medium">{{ claim()!.reportedDate }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Monto reclamado</dt><dd class="font-medium">{{ claim()!.claimedAmount | currency }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Monto aprobado</dt><dd class="font-medium">{{ claim()!.approvedAmount ?? '-' }}</dd></div>
                <div><dt class="text-xs text-claims-muted">Notas de peritaje</dt><dd class="font-medium">{{ claim()!.peritajeNotes ?? '-' }}</dd></div>
              </dl>
            </div>

            <div class="grid gap-4 lg:grid-cols-2">
              <div class="rounded border border-claims-border bg-claims-surface p-5">
                <h2 class="mb-4 text-base font-semibold">Datos de la poliza</h2>
                <dl class="grid gap-4">
                  <div><dt class="text-xs text-claims-muted">Numero</dt><dd class="font-medium">{{ claim()!.policy.policyNumber }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Ramo</dt><dd class="font-medium">{{ branchLabel(claim()!.policy.branch) }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Vigencia</dt><dd class="font-medium">{{ claim()!.policy.startDate }} - {{ claim()!.policy.endDate }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Prima</dt><dd class="font-medium">{{ claim()!.policy.premium | currency }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Suma asegurada</dt><dd class="font-medium">{{ claim()!.policy.insuredAmount | currency }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Estado</dt><dd class="font-medium">{{ policyStatusLabel(claim()!.policy.status) }}</dd></div>
                </dl>
              </div>

              <div class="rounded border border-claims-border bg-claims-surface p-5">
                <h2 class="mb-4 text-base font-semibold">Datos del asegurado</h2>
                <dl class="grid gap-4">
                  <div><dt class="text-xs text-claims-muted">Nombre</dt><dd class="font-medium">{{ claim()!.insuredParty.fullName }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Documento</dt><dd class="font-medium">{{ claim()!.insuredParty.maskedDocumentId }}</dd></div>
                  <div><dt class="text-xs text-claims-muted">Email</dt><dd class="font-medium">{{ claim()!.insuredParty.maskedEmail }}</dd></div>
                </dl>
              </div>
            </div>

            <div *appHasRole="'ADJUSTER'" class="rounded border border-claims-border bg-claims-surface p-5">
              <h2 class="mb-4 text-base font-semibold">Acciones del Liquidador</h2>
              <div class="flex flex-wrap gap-2">
                @if (claim()!.status === 'REPORTED') {
                  <button mat-flat-button type="button" [disabled]="actionLoading()" (click)="startReview()">Iniciar revisión</button>
                }

                @if (claim()!.status === 'UNDER_REVIEW') {
                  <form class="grid w-full gap-3 md:grid-cols-[180px_1fr_auto_auto]" [formGroup]="decisionForm">
                    <mat-form-field appearance="outline">
                      <mat-label>Monto aprobado</mat-label>
                      <input matInput type="number" formControlName="approvedAmount" />
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Notas de peritaje</mat-label>
                      <input matInput formControlName="peritajeNotes" />
                    </mat-form-field>
                    <button mat-flat-button type="button" [disabled]="actionLoading()" (click)="approve()">Aprobar</button>
                    <button mat-stroked-button type="button" [disabled]="actionLoading()" (click)="reject()">Rechazar</button>
                  </form>
                }

                @if (claim()!.status === 'APPROVED') {
                  <button mat-flat-button type="button" [disabled]="actionLoading()" (click)="pay()">Pagar</button>
                }
              </div>
            </div>
          </div>

          <aside class="rounded border border-claims-border bg-claims-surface p-5">
            <h2 class="mb-4 text-base font-semibold">Línea de auditoría</h2>
            <ol class="space-y-4">
              @for (item of claim()!.history; track item.historyId) {
                <li class="border-l-2 border-claims-blue-soft pl-4">
                  <div class="text-sm font-semibold">{{ statusLabel(item.previousStatus) }} -> {{ statusLabel(item.newStatus) }}</div>
                  <div class="text-xs text-claims-muted">{{ item.changedAt }} por {{ item.changedBy }}</div>
                  @if (item.reason) {
                    <div class="mt-1 text-sm text-claims-ink">{{ item.reason }}</div>
                  }
                </li>
              }
            </ol>
          </aside>
        </section>
      }
    </section>
  `,
})
export class ClaimDetailPageComponent implements OnInit {
  private readonly api = inject(ClaimsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly actionLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly claim = signal<ClaimDetail | null>(null);
  readonly statusClass = statusClass;
  readonly statusLabel = getStatusLabel;
  readonly branchLabel = getBranchLabel;
  readonly claimTypeLabel = getClaimTypeLabel;
  readonly decisionForm = this.fb.nonNullable.group({
    approvedAmount: [0, [Validators.required, Validators.min(1)]],
    peritajeNotes: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.error.set('No se encontró el identificador del siniestro.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.api
      .getClaim(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claim) => this.claim.set(claim),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }

  startReview(): void {
    this.runAction((id) => this.api.startReview(id));
  }

  approve(): void {
    if (this.decisionForm.invalid) {
      this.decisionForm.markAllAsTouched();
      return;
    }

    this.runAction((id) =>
      this.api.approve(id, this.decisionForm.controls.approvedAmount.value, this.decisionForm.controls.peritajeNotes.value),
    );
  }

  reject(): void {
    if (!this.decisionForm.controls.peritajeNotes.value.trim()) {
      this.decisionForm.controls.peritajeNotes.markAsTouched();
      return;
    }

    this.runAction((id) => this.api.reject(id, this.decisionForm.controls.peritajeNotes.value));
  }

  pay(): void {
    this.runAction((id) => this.api.pay(id));
  }

  policyStatusLabel(status: string): string {
    return {
      ACTIVE: 'Activa',
      EXPIRED: 'Vencida',
      CANCELLED: 'Cancelada',
    }[status] ?? status;
  }

  private runAction(action: (id: string) => ReturnType<ClaimsApiService['pay']>): void {
    const claim = this.claim();

    if (!claim) {
      return;
    }

    this.actionLoading.set(true);
    this.error.set(null);
    action(claim.claimId)
      .pipe(finalize(() => this.actionLoading.set(false)))
      .subscribe({
        next: () => this.load(),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }
}
