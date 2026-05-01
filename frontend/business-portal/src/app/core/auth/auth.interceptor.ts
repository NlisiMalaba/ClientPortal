import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, switchMap, throwError } from 'rxjs';

import { AuthSessionService } from './auth-session.service';
import { TokenStorageService } from './token-storage.service';

export const authInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> => {
  const tokenStorage = inject(TokenStorageService);
  const authSessionService = inject(AuthSessionService);
  const router = inject(Router);

  const accessToken = tokenStorage.getAccessToken();
  const isRefreshRequest = authSessionService.isRefreshRequest(request.url);
  const requestWithToken =
    accessToken !== null && !isRefreshRequest
      ? withBearerToken(request, accessToken)
      : request;

  return next(requestWithToken).pipe(
    catchError((error: unknown) => {
      const httpError = error as HttpErrorResponse;
      if (httpError.status !== 401 || isRefreshRequest) {
        return throwError(() => error);
      }

      return authSessionService.refreshAccessToken().pipe(
        switchMap((refreshedAccessToken) => {
          if (refreshedAccessToken === null) {
            authSessionService.clearSession();
            void router.navigate(['/auth']);
            return throwError(() => error);
          }

          return next(withBearerToken(request, refreshedAccessToken));
        }),
        catchError((refreshError) => {
          authSessionService.clearSession();
          void router.navigate(['/auth']);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};

function withBearerToken(
  request: HttpRequest<unknown>,
  accessToken: string,
): HttpRequest<unknown> {
  return request.clone({
    setHeaders: { Authorization: `Bearer ${accessToken}` },
  });
}
