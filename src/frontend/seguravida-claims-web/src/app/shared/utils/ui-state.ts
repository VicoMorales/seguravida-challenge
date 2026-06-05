import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.[0] ?? body?.message ?? 'Request failed';
  }

  return 'Unexpected error';
}

export function statusClass(status: string): string {
  return {
    REPORTED: 'status-reported',
    UNDER_REVIEW: 'status-under-review',
    APPROVED: 'status-approved',
    REJECTED: 'status-rejected',
    PAID: 'status-paid',
  }[status] ?? 'status-reported';
}
