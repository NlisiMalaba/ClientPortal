import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult } from '../models';

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantName?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface AcceptInvitationRequest {
  invitationToken: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly basePath = '/api/v1/auth';

  constructor(private readonly apiClient: ApiClientService) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.apiClient.post<LoginResponse, LoginRequest>(
      `${this.basePath}/login`,
      request,
    );
  }

  refresh(request: RefreshRequest): Observable<LoginResponse> {
    return this.apiClient.post<LoginResponse, RefreshRequest>(
      `${this.basePath}/refresh`,
      request,
    );
  }

  logout(refreshToken: string): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, RefreshRequest>(
      `${this.basePath}/logout`,
      { refreshToken },
    );
  }

  register(request: RegisterRequest): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, RegisterRequest>(
      `${this.basePath}/register`,
      request,
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, ForgotPasswordRequest>(
      `${this.basePath}/forgot-password`,
      request,
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, ResetPasswordRequest>(
      `${this.basePath}/reset-password`,
      request,
    );
  }

  acceptInvitation(
    request: AcceptInvitationRequest,
  ): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, AcceptInvitationRequest>(
      `${this.basePath}/accept-invitation`,
      request,
    );
  }
}
