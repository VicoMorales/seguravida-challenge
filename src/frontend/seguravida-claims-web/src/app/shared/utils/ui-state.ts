import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.[0] ?? body?.message ?? 'No se pudo completar la solicitud.';
  }

  return 'Ocurrió un error inesperado.';
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

export function statusLabel(status: string): string {
  return {
    REPORTED: 'Reportado',
    UNDER_REVIEW: 'En revisión',
    APPROVED: 'Aprobado',
    REJECTED: 'Rechazado',
    PAID: 'Pagado',
  }[status] ?? status;
}

export function branchLabel(branch: string): string {
  return {
    AUTO: 'Vehicular',
    LIFE: 'Vida',
    HEALTH: 'Salud',
    HOME: 'Hogar',
  }[branch] ?? branch;
}

export function claimTypeLabel(type: string): string {
  return {
    ACCIDENT: 'Accidente',
    THEFT: 'Robo',
    MEDICAL: 'Médico',
    DEATH: 'Fallecimiento',
    PROPERTY_DAMAGE: 'Daños materiales',
  }[type] ?? type;
}

export function roleLabel(role: string): string {
  return {
    OPERATOR: 'Operador',
    ADJUSTER: 'Ajustador',
    AUDITOR: 'Auditor',
    NO_ROLE: 'Sin rol',
  }[role] ?? role;
}
