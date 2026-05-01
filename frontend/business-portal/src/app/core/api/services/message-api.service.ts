import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface MessageThreadSummary {
  id: string;
  subject?: string;
  unreadCount: number;
  updatedAtUtc: string;
  [key: string]: unknown;
}

export interface MessageItem {
  id: string;
  threadId: string;
  body: string;
  sentAtUtc: string;
  senderUserId: string;
  [key: string]: unknown;
}

export interface MessageThreadQuery {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface SendMessageRequest {
  body: string;
  attachmentKeys?: string[];
}

@Injectable({ providedIn: 'root' })
export class MessageApiService {
  private readonly basePath = '/api/v1/messages';

  constructor(private readonly apiClient: ApiClientService) {}

  getThreads(query?: MessageThreadQuery): Observable<PagedResult<MessageThreadSummary>> {
    return this.apiClient.get<PagedResult<MessageThreadSummary>>(
      `${this.basePath}/threads`,
      query,
    );
  }

  getThreadMessages(threadId: string): Observable<MessageItem[]> {
    return this.apiClient.get<MessageItem[]>(
      `${this.basePath}/threads/${threadId}/messages`,
    );
  }

  sendMessage(
    threadId: string,
    request: SendMessageRequest,
  ): Observable<ApiOperationResult> {
    return this.apiClient.post<ApiOperationResult, SendMessageRequest>(
      `${this.basePath}/threads/${threadId}/messages`,
      request,
    );
  }

  markThreadRead(threadId: string): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, Record<string, never>>(
      `${this.basePath}/threads/${threadId}/read`,
      {},
    );
  }
}
