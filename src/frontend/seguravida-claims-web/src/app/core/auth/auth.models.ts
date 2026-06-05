export type UserRole = 'OPERATOR' | 'ADJUSTER' | 'AUDITOR';

export interface AuthSession {
  accessToken: string;
  email: string;
  role: UserRole;
  displayName: string;
}

export interface LoginResponse {
  accessToken: string;
  email: string;
  role: UserRole;
  displayName: string;
}
