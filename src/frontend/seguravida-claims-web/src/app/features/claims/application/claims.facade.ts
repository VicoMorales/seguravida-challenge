import { computed, inject, Injectable, signal } from '@angular/core';
import { finalize } from 'rxjs';

import { errorMessage } from '../../../shared/utils/ui-state';
import { ClaimsApiService } from '../data-access/claims-api.service';
import { ClaimFilters, ClaimListItem, PagedResult } from '../domain/claim.models';

@Injectable()
export class ClaimsFacade {
  private readonly api = inject(ClaimsApiService);
  private readonly result = signal<PagedResult<ClaimListItem> | null>(null);

  readonly filters = signal<ClaimFilters>({ page: 1, pageSize: 10 });
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly claims = computed(() => this.result()?.items ?? []);
  readonly totalCount = computed(() => this.result()?.totalCount ?? 0);

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getClaims(this.filters())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.result.set(result),
        error: (error) => this.error.set(errorMessage(error)),
      });
  }

  patchFilters(filters: Partial<ClaimFilters>): void {
    this.filters.update((current) => ({ ...current, ...filters, page: filters.page ?? 1 }));
    this.load();
  }
}
