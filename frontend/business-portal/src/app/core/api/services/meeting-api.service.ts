import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface MeetingSummary {
  id: string;
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  status: string;
  [key: string]: unknown;
}

export interface MeetingDetail extends MeetingSummary {
  description?: string;
  joinUrl?: string;
}

export interface MeetingListQuery {
  fromUtc?: string;
  toUtc?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateMeetingRequest {
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  participantClientIds?: string[];
  description?: string;
}

export interface UpdateMeetingRequest {
  title?: string;
  startsAtUtc?: string;
  endsAtUtc?: string;
  description?: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class MeetingApiService {
  private readonly basePath = '/api/v1/meetings';

  constructor(private readonly apiClient: ApiClientService) {}

  getMeetings(query?: MeetingListQuery): Observable<PagedResult<MeetingSummary>> {
    return this.apiClient.get<PagedResult<MeetingSummary>>(`${this.basePath}/`, query);
  }

  getMeetingById(meetingId: string): Observable<MeetingDetail> {
    return this.apiClient.get<MeetingDetail>(`${this.basePath}/${meetingId}`);
  }

  scheduleMeeting(request: CreateMeetingRequest): Observable<MeetingDetail> {
    return this.apiClient.post<MeetingDetail, CreateMeetingRequest>(
      `${this.basePath}/`,
      request,
    );
  }

  updateMeeting(
    meetingId: string,
    request: UpdateMeetingRequest,
  ): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, UpdateMeetingRequest>(
      `${this.basePath}/${meetingId}`,
      request,
    );
  }

  cancelMeeting(meetingId: string): Observable<ApiOperationResult> {
    return this.apiClient.delete<ApiOperationResult>(`${this.basePath}/${meetingId}`);
  }
}
