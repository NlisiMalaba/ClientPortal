import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../api-client.service';
import { ApiOperationResult, PagedResult } from '../models';

export interface ProjectSummary {
  id: string;
  name: string;
  status: string;
  clientId: string;
  [key: string]: unknown;
}

export interface ProjectDetail extends ProjectSummary {
  description?: string;
  startDateUtc?: string;
  dueDateUtc?: string;
}

export interface ProjectListQuery {
  search?: string;
  status?: string;
  clientId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateProjectRequest {
  name: string;
  clientId: string;
  description?: string;
  startDateUtc?: string;
  dueDateUtc?: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  status?: string;
  dueDateUtc?: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectApiService {
  private readonly basePath = '/api/v1/projects';

  constructor(private readonly apiClient: ApiClientService) {}

  getProjects(query?: ProjectListQuery): Observable<PagedResult<ProjectSummary>> {
    return this.apiClient.get<PagedResult<ProjectSummary>>(`${this.basePath}/`, query);
  }

  getProjectById(projectId: string): Observable<ProjectDetail> {
    return this.apiClient.get<ProjectDetail>(`${this.basePath}/${projectId}`);
  }

  createProject(request: CreateProjectRequest): Observable<ProjectDetail> {
    return this.apiClient.post<ProjectDetail, CreateProjectRequest>(
      `${this.basePath}/`,
      request,
    );
  }

  updateProject(
    projectId: string,
    request: UpdateProjectRequest,
  ): Observable<ApiOperationResult> {
    return this.apiClient.put<ApiOperationResult, UpdateProjectRequest>(
      `${this.basePath}/${projectId}`,
      request,
    );
  }

  deleteProject(projectId: string): Observable<ApiOperationResult> {
    return this.apiClient.delete<ApiOperationResult>(`${this.basePath}/${projectId}`);
  }
}
