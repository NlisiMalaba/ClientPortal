import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface ClientSummary {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  status: string;
  [key: string]: unknown;
}

export interface ClientDetail extends ClientSummary {
  phone?: string;
  companyName?: string;
}

export interface ClientListQuery {
  search?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface InviteClientRequest {
  email: string;
  firstName: string;
  lastName: string;
}

export interface UpdateClientRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class ClientApiService {
  private readonly basePath = '/api/v1/clients';

  constructor(private readonly apiClient: ApiClientService) {}

  getClients(query?: ClientListQuery): Observable<PagedResult<ClientSummary>> {
    return this.apiClient.get<PagedResult<ClientSummary>>(`${this.basePath}/`, query);
  }

  getClientById(clientId: string): Observable<ClientDetail> {
    return this.apiClient.get<ClientDetail>(`${this.basePath}/${clientId}`);
  }

  inviteClient(request: InviteClientRequest): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, InviteClientRequest>(
      `${this.basePath}/invite`,
      request,
    );
  }

  updateClient(
    clientId: string,
    request: UpdateClientRequest,
  ): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, UpdateClientRequest>(
      `${this.basePath}/${clientId}`,
      request,
    );
  }

  deactivateClient(clientId: string): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, Record<string, never>>(
      `${this.basePath}/${clientId}/deactivate`,
      {},
    );
  }
}
