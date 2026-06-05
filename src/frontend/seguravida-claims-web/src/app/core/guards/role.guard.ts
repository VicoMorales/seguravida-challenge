import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { UserRole } from '../auth/auth.models';
import { AuthService } from '../auth/auth.service';

export function roleGuard(roles: UserRole[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);

    if (auth.hasAnyRole(roles)) {
      return true;
    }

    return inject(Router).parseUrl('/claims');
  };
}
