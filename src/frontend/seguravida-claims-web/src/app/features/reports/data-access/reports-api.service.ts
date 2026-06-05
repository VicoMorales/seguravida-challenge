import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { API_BASE_URL } from '../../../core/config/api.config';
import { ClaimsSummaryRow } from '../domain/report.models';

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  getClaimsSummary(fromDate?: string, toDate?: string) {
    let params = new HttpParams();

    if (fromDate) {
      params = params.set('fromDate', fromDate);
    }

    if (toDate) {
      params = params.set('toDate', toDate);
    }

    return this.http.get<ClaimsSummaryRow[]>(`${this.apiBaseUrl}/reports/claims-summary`, { params });
  }
}
