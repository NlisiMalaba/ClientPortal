import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { authInterceptor } from './core/auth/auth.interceptor';
import { globalHttpErrorInterceptor } from './core/http/global-http-error.interceptor';
import { tenantInterceptor } from './core/tenant/tenant.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(
      withInterceptors([
        authInterceptor,
        tenantInterceptor,
        globalHttpErrorInterceptor,
      ]),
    ),
    provideRouter(routes),
  ],
};
