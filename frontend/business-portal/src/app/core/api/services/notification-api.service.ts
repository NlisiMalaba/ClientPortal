import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAtUtc: string;
  [key: string]: unknown;
}

export interface NotificationPreference {
  channel: string;
  enabled: boolean;
  [key: string]: unknown;
}

export interface NotificationQuery {
  unreadOnly?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly basePath = '/api/v1/notifications';

  constructor(private readonly apiClient: ApiClientService) {}

  getNotifications(
    query?: NotificationQuery,
  ): Observable<PagedResult<NotificationItem>> {
    return this.apiClient.get<PagedResult<NotificationItem>>(
      `${this.basePath}/`,
      query,
    );
  }

  markAsRead(notificationId: string): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, Record<string, never>>(
      `${this.basePath}/${notificationId}/read`,
      {},
    );
  }

  getPreferences(): Observable<NotificationPreference[]> {
    return this.apiClient.get<NotificationPreference[]>(
      `${this.basePath}/preferences`,
    );
  }

  updatePreferences(
    preferences: NotificationPreference[],
  ): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, NotificationPreference[]>(
      `${this.basePath}/preferences`,
      preferences,
    );
  }
}
