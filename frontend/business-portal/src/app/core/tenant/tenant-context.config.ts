import { InjectionToken } from '@angular/core';

export interface TenantContextConfig {
  tenantHeaderName: string;
  tenantStorageKey: string;
}

export const TENANT_CONTEXT_CONFIG = new InjectionToken<TenantContextConfig>(
  'TENANT_CONTEXT_CONFIG',
  {
    providedIn: 'root',
    factory: (): TenantContextConfig => ({
      tenantHeaderName: 'X-Tenant-Id',
      tenantStorageKey: 'bp_tenant_id',
    }),
  },
);
