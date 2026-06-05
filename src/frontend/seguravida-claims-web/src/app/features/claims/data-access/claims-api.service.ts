import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { API_BASE_URL } from '../../../core/config/api.config';
import { ClaimDetail, ClaimFilters, ClaimListItem, CreateClaimPayload, PagedResult } from '../domain/claim.models';

@Injectable({ providedIn: 'root' })
export class ClaimsApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  getClaims(filters: ClaimFilters) {
    let params = new HttpParams().set('page', filters.page).set('pageSize', filters.pageSize);

    if (filters.search) {
      params = params.set('search', filters.search);
    }

    if (filters.status) {
      params = params.set('status', filters.status);
    }

    if (filters.branch) {
      params = params.set('branch', filters.branch);
    }

    if (filters.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }

    if (filters.toDate) {
      params = params.set('toDate', filters.toDate);
    }

    return this.http.get<PagedResult<ClaimListItem>>(`${this.apiBaseUrl}/claims`, { params });
  }

  getClaim(id: string) {
    return this.http.get<ClaimDetail>(`${this.apiBaseUrl}/claims/${id}`);
  }

  createClaim(payload: CreateClaimPayload) {
    return this.http.post<{ claimId: string }>(`${this.apiBaseUrl}/claims`, payload);
  }

  startReview(id: string) {
    return this.http.post<void>(`${this.apiBaseUrl}/claims/${id}/start-review`, {});
  }

  approve(id: string, approvedAmount: number, peritajeNotes: string) {
    return this.http.post<void>(`${this.apiBaseUrl}/claims/${id}/approve`, { approvedAmount, peritajeNotes });
  }

  reject(id: string, peritajeNotes: string) {
    return this.http.post<void>(`${this.apiBaseUrl}/claims/${id}/reject`, { peritajeNotes });
  }

  pay(id: string) {
    return this.http.post<void>(`${this.apiBaseUrl}/claims/${id}/pay`, {});
  }
}
