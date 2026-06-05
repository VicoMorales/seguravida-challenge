export type ClaimStatus = 'REPORTED' | 'UNDER_REVIEW' | 'APPROVED' | 'REJECTED' | 'PAID';
export type ClaimBranch = 'AUTO' | 'LIFE' | 'HEALTH' | 'HOME';
export type ClaimType = 'ACCIDENT' | 'THEFT' | 'MEDICAL' | 'DEATH' | 'PROPERTY_DAMAGE';

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ClaimListItem {
  claimId: string;
  claimNumber: string;
  policyNumber: string;
  branch: ClaimBranch;
  type: ClaimType;
  status: ClaimStatus;
  incidentDate: string;
  reportedDate: string;
  claimedAmount: number;
  approvedAmount: number | null;
}

export interface ClaimStatusHistory {
  historyId: string;
  previousStatus: ClaimStatus;
  newStatus: ClaimStatus;
  changedBy: string;
  changedAt: string;
  reason: string | null;
}

export interface ClaimDetail {
  claimId: string;
  claimNumber: string;
  policyId: string;
  policyNumber: string;
  branch: ClaimBranch;
  type: ClaimType;
  description: string;
  incidentDate: string;
  reportedDate: string;
  claimedAmount: number;
  approvedAmount: number | null;
  status: ClaimStatus;
  peritajeNotes: string | null;
  history: ClaimStatusHistory[];
}

export interface ClaimFilters {
  page: number;
  pageSize: number;
  search?: string;
  status?: ClaimStatus | '';
  branch?: ClaimBranch | '';
  fromDate?: string;
  toDate?: string;
}

export interface CreateClaimPayload {
  policyId: string;
  type: ClaimType;
  incidentDate: string;
  reportedDate: string;
  claimedAmount: number;
  description: string;
}
