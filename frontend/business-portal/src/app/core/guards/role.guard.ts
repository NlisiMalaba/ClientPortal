import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { AuthContextService } from '../auth/auth-context.service';

export const roleGuard: CanActivateFn = (route) => {
  const authContext = inject(AuthContextService);
  const router = inject(Router);

  const requiredRoles = (route.data['roles'] as string[] | undefined) ?? [];
  if (requiredRoles.length === 0) {
    return true;
  }

  const currentRoles = authContext.getRoles();
  const hasRequiredRole = requiredRoles.some((requiredRole) =>
    currentRoles.includes(requiredRole),
  );

  if (hasRequiredRole) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
