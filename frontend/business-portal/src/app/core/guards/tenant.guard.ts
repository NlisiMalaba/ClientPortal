import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { TenantContextService } from '../tenant/tenant-context.service';

export const tenantGuard: CanActivateFn = () => {
  const tenantContextService = inject(TenantContextService);
  const router = inject(Router);

  const tenantId = tenantContextService.getTenantId();
  if (tenantId !== null && tenantId.trim() !== '') {
    return true;
  }

  return router.createUrlTree(['/auth']);
};
