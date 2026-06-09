import { CurrencyPipe, DecimalPipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

import {
  branchLabel as getBranchLabel,
  errorMessage,
  statusLabel as getStatusLabel,
} from '../../../shared/utils/ui-state';
import { ReportsApiService } from '../data-access/reports-api.service';
import { ClaimsSummaryRow } from '../domain/report.models';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { formatDateMat } from '../../../shared/utils/formatDate';

Chart.register(...registerables);

const STATUS_COLORS: Record<string, string> = {
  REPORTED: '#64748b',
  UNDER_REVIEW: '#f59e0b',
  APPROVED: '#3b82f6',
  REJECTED: '#ef4444',
  PAID: '#10b981',
};

const BRANCH_COLORS: Record<string, string> = {
  AUTO: '#2563eb',
  LIFE: '#7c3aed',
  HEALTH: '#0ea5e9',
  HOME: '#f97316',
};

const STATUS_ORDER = ['REPORTED', 'UNDER_REVIEW', 'APPROVED', 'REJECTED', 'PAID'];
const BRANCH_ORDER = ['AUTO', 'LIFE', 'HEALTH', 'HOME'];

interface Kpi {
  label: string;
  value: string;
  hint: string;
  icon: string;
  tone: 'indigo' | 'emerald' | 'amber' | 'rose';
}

@Component({
  selector: 'app-reports-page',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  template: `
    <section class="space-y-6">
      <header class="flex flex-col gap-1">
        <h1 class="text-2xl font-semibold text-slate-950">Resumen de siniestros</h1>
        <p class="text-sm text-slate-600">
          Indicadores agregados por ramo y estado, con monto pagado en el período seleccionado.
        </p>
      </header>

      <section class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <form
          class="grid gap-3 md:grid-cols-[200px_200px_auto]"
          [formGroup]="form"
          (ngSubmit)="load()"
        >
          <mat-form-field appearance="outline">
            <mat-label>Fecha desde</mat-label>
            <input matInput [matDatepicker]="fromDatePicker" formControlName="fromDate" />
            <mat-datepicker-toggle matSuffix [for]="fromDatePicker"></mat-datepicker-toggle>
            <mat-datepicker #fromDatePicker></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Fecha hasta</mat-label>
            <input matInput [matDatepicker]="toDatePicker" formControlName="toDate" />
            <mat-datepicker-toggle matSuffix [for]="toDatePicker"></mat-datepicker-toggle>
            <mat-datepicker #toDatePicker></mat-datepicker>
          </mat-form-field>
          <button mat-flat-button type="submit" class="!h-14">Aplicar</button>
        </form>
      </section>

      @if (loading()) {
        <div class="grid min-h-72 place-items-center rounded-lg border border-slate-200 bg-white">
          <mat-spinner diameter="36" />
        </div>
      } @else if (error()) {
        <div class="rounded-lg border border-red-200 bg-red-50 p-6 text-sm text-red-700">
          {{ error() }}
        </div>
      } @else {
        <section class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          @for (kpi of kpis(); track kpi.label) {
            <article
              class="flex items-start gap-4 rounded-lg border border-slate-200 bg-white p-4 shadow-sm"
            >
              <span
                class="grid h-11 w-11 place-items-center rounded-lg"
                [class.bg-indigo-50]="kpi.tone === 'indigo'"
                [class.text-indigo-600]="kpi.tone === 'indigo'"
                [class.bg-emerald-50]="kpi.tone === 'emerald'"
                [class.text-emerald-600]="kpi.tone === 'emerald'"
                [class.bg-amber-50]="kpi.tone === 'amber'"
                [class.text-amber-600]="kpi.tone === 'amber'"
                [class.bg-rose-50]="kpi.tone === 'rose'"
                [class.text-rose-600]="kpi.tone === 'rose'"
              >
                <mat-icon>{{ kpi.icon }}</mat-icon>
              </span>
              <div class="min-w-0">
                <p class="text-xs font-medium uppercase tracking-wide text-slate-500">
                  {{ kpi.label }}
                </p>
                <p class="mt-1 truncate text-2xl font-semibold text-slate-900">{{ kpi.value }}</p>
                <p class="text-xs text-slate-500">{{ kpi.hint }}</p>
              </div>
            </article>
          }
        </section>

        <section class="grid gap-4 lg:grid-cols-5">
          <article
            class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm lg:col-span-2"
          >
            <header class="mb-4">
              <h2 class="text-sm font-semibold text-slate-900">Distribución por estado</h2>
              <p class="text-xs text-slate-500">Participación sobre el total de siniestros.</p>
            </header>
            <div class="relative h-64">
              <canvas #statusChart></canvas>
            </div>
            <ul class="mt-4 space-y-2 text-xs">
              @for (item of statusBreakdown(); track item.status) {
                <li class="flex items-center gap-2">
                  <span class="h-2.5 w-2.5 rounded-full" [style.background]="item.color"></span>
                  <span class="flex-1 text-slate-700">{{ item.label }}</span>
                  <span class="font-medium text-slate-900">{{ item.total | number }}</span>
                  <span class="w-12 text-right text-slate-500">
                    {{ item.percent | number: '1.0-1' }}%
                  </span>
                </li>
              }
            </ul>
          </article>

          <article
            class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm lg:col-span-3"
          >
            <header class="mb-4 flex items-baseline justify-between">
              <div>
                <h2 class="text-sm font-semibold text-slate-900">Monto pagado por ramo</h2>
                <p class="text-xs text-slate-500">Total pagado agrupado por línea de negocio.</p>
              </div>
              <span class="text-xs font-medium text-slate-500">
                Total: {{ totalPaid() | currency }}
              </span>
            </header>
            <div class="relative h-64">
              <canvas #branchChart></canvas>
            </div>
          </article>
        </section>

        <section class="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <header class="border-b border-slate-200 px-4 py-3">
            <h2 class="text-sm font-semibold text-slate-900">Detalle por ramo y estado</h2>
            <p class="text-xs text-slate-500">Cruce completo del período seleccionado.</p>
          </header>
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
              <td mat-cell *matCellDef="let row">{{ row.totalClaims | number }}</td>
            </ng-container>
            <ng-container matColumnDef="paidAmount">
              <th mat-header-cell *matHeaderCellDef>Monto pagado</th>
              <td mat-cell *matCellDef="let row">{{ row.paidAmount | currency }}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
        </section>
      }
    </section>
  `,
})
export class ReportsPageComponent implements OnInit, AfterViewInit, OnDestroy {
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

  readonly totalClaims = computed(() =>
    this.rows().reduce((acc, r) => acc + r.totalClaims, 0),
  );
  readonly totalPaid = computed(() =>
    this.rows().reduce((acc, r) => acc + r.paidAmount, 0),
  );
  readonly pendingClaims = computed(() =>
    this.rows()
      .filter((r) => r.status === 'REPORTED' || r.status === 'UNDER_REVIEW')
      .reduce((acc, r) => acc + r.totalClaims, 0),
  );
  readonly rejectedClaims = computed(() =>
    this.rows()
      .filter((r) => r.status === 'REJECTED')
      .reduce((acc, r) => acc + r.totalClaims, 0),
  );

  readonly statusBreakdown = computed(() => {
    const totals = new Map<string, number>();
    for (const r of this.rows()) {
      totals.set(r.status, (totals.get(r.status) ?? 0) + r.totalClaims);
    }
    const total = this.totalClaims() || 1;
    const ordered = [...totals.keys()].sort(
      (a, b) => STATUS_ORDER.indexOf(a) - STATUS_ORDER.indexOf(b),
    );
    return ordered.map((status) => ({
      status,
      label: getStatusLabel(status),
      color: STATUS_COLORS[status] ?? '#94a3b8',
      total: totals.get(status) ?? 0,
      percent: ((totals.get(status) ?? 0) / total) * 100,
    }));
  });

  readonly kpis = computed<Kpi[]>(() => [
    {
      label: 'Total siniestros',
      value: new Intl.NumberFormat('es-CO').format(this.totalClaims()),
      hint: 'Casos registrados',
      icon: 'inventory_2',
      tone: 'indigo',
    },
    {
      label: 'Monto pagado',
      value: new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'USD',
        maximumFractionDigits: 0,
      }).format(this.totalPaid()),
      hint: 'Suma de pagos efectuados',
      icon: 'payments',
      tone: 'emerald',
    },
    {
      label: 'Pendientes',
      value: new Intl.NumberFormat('es-CO').format(this.pendingClaims()),
      hint: 'Reportados o en revisión',
      icon: 'hourglass_top',
      tone: 'amber',
    },
    {
      label: 'Rechazados',
      value: new Intl.NumberFormat('es-CO').format(this.rejectedClaims()),
      hint: 'No procedentes',
      icon: 'block',
      tone: 'rose',
    },
  ]);

  private readonly statusCanvas = viewChild<ElementRef<HTMLCanvasElement>>('statusChart');
  private readonly branchCanvas = viewChild<ElementRef<HTMLCanvasElement>>('branchChart');
  private statusChart?: Chart;
  private branchChart?: Chart;
  private viewReady = false;

  constructor() {
    effect(() => {
      this.rows();
      if (this.viewReady) {
        queueMicrotask(() => this.renderCharts());
      }
    });
  }

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    queueMicrotask(() => this.renderCharts());
  }

  ngOnDestroy(): void {
    this.statusChart?.destroy();
    this.branchChart?.destroy();
  }

  load(): void {
    const { fromDate, toDate } = this.form.getRawValue();
    const formattedFromDate = formatDateMat(fromDate);
    const formattedToDate = formatDateMat(toDate);
    this.loading.set(true);
    this.error.set(null);
    this.api
      .getClaimsSummary(formattedFromDate, formattedToDate)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.rows.set(rows),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }

  private renderCharts(): void {
    this.renderStatusChart();
    this.renderBranchChart();
  }

  private renderStatusChart(): void {
    const canvas = this.statusCanvas()?.nativeElement;
    if (!canvas) return;

    const breakdown = this.statusBreakdown();
    const config: ChartConfiguration<'doughnut'> = {
      type: 'doughnut',
      data: {
        labels: breakdown.map((b) => b.label),
        datasets: [
          {
            data: breakdown.map((b) => b.total),
            backgroundColor: breakdown.map((b) => b.color),
            borderWidth: 0,
            hoverOffset: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '68%',
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (ctx) => {
                const value = Number(ctx.parsed) || 0;
                const total = this.totalClaims() || 1;
                const pct = ((value / total) * 100).toFixed(1);
                return ` ${ctx.label}: ${value} (${pct}%)`;
              },
            },
          },
        },
      },
    };

    if (this.statusChart) {
      this.statusChart.data = config.data;
      this.statusChart.update();
    } else {
      this.statusChart = new Chart(canvas, config);
    }
  }

  private renderBranchChart(): void {
    const canvas = this.branchCanvas()?.nativeElement;
    if (!canvas) return;

    const totals = new Map<string, number>();
    for (const r of this.rows()) {
      totals.set(r.branch, (totals.get(r.branch) ?? 0) + r.paidAmount);
    }
    const branches = [...totals.keys()].sort(
      (a, b) => BRANCH_ORDER.indexOf(a) - BRANCH_ORDER.indexOf(b),
    );

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels: branches.map((b) => getBranchLabel(b)),
        datasets: [
          {
            label: 'Monto pagado',
            data: branches.map((b) => totals.get(b) ?? 0),
            backgroundColor: branches.map((b) => BRANCH_COLORS[b] ?? '#94a3b8'),
            borderRadius: 6,
            maxBarThickness: 56,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (ctx) =>
                ' ' +
                new Intl.NumberFormat('es-CO', {
                  style: 'currency',
                  currency: 'USD',
                  maximumFractionDigits: 0,
                }).format(Number(ctx.parsed.y) || 0),
            },
          },
        },
        scales: {
          x: { grid: { display: false } },
          y: {
            beginAtZero: true,
            grid: { color: '#e2e8f0' },
            ticks: {
              callback: (value) =>
                new Intl.NumberFormat('es-CO', { notation: 'compact' }).format(Number(value)),
            },
          },
        },
      },
    };

    if (this.branchChart) {
      this.branchChart.data = config.data;
      this.branchChart.update();
    } else {
      this.branchChart = new Chart(canvas, config);
    }
  }
}
