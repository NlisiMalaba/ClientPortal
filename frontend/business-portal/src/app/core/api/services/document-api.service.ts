import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface DocumentSummary {
  id: string;
  fileName: string;
  contentType: string;
  status: string;
  uploadedAtUtc: string;
  [key: string]: unknown;
}

export interface DocumentListQuery {
  projectId?: string;
  clientId?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface UploadUrlRequest {
  fileName: string;
  contentType: string;
  fileSize: number;
}

export interface UploadUrlResponse {
  uploadUrl: string;
  key: string;
}

export interface ConfirmUploadRequest {
  key: string;
  fileName: string;
  contentType: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentApiService {
  private readonly basePath = '/api/v1/documents';

  constructor(private readonly apiClient: ApiClientService) {}

  getDocuments(query?: DocumentListQuery): Observable<PagedResult<DocumentSummary>> {
    return this.apiClient.get<PagedResult<DocumentSummary>>(`${this.basePath}/`, query);
  }

  getUploadUrl(request: UploadUrlRequest): Observable<UploadUrlResponse> {
    return this.apiClient.post<UploadUrlResponse, UploadUrlRequest>(
      `${this.basePath}/upload-url`,
      request,
    );
  }

  confirmUpload(request: ConfirmUploadRequest): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, ConfirmUploadRequest>(
      `${this.basePath}/confirm-upload`,
      request,
    );
  }

  getDownloadUrl(documentId: string): Observable<{ downloadUrl: string }> {
    return this.apiClient.get<{ downloadUrl: string }>(
      `${this.basePath}/${documentId}/download`,
    );
  }
}
