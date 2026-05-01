import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';

import {
  CreateProjectRequest,
  ProjectApiService,
  ProjectDetail,
  ProjectListQuery,
  ProjectSummary,
  UpdateProjectRequest,
} from '../api/services/project-api.service';

interface ProjectState {
  projects: ProjectSummary[];
  selectedProject: ProjectDetail | null;
  totalCount: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: ProjectState = {
  projects: [],
  selectedProject: null,
  totalCount: 0,
  isLoading: false,
  error: null,
};

export const ProjectStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, projectApiService = inject(ProjectApiService)) => ({
    async loadProjects(query?: ProjectListQuery): Promise<void> {
      patchState(store, { isLoading: true, error: null });
      try {
        const result = await firstValueFrom(projectApiService.getProjects(query));
        patchState(store, {
          projects: result.items,
          totalCount: result.totalCount,
        });
      } catch (error) {
        patchState(store, { error: readErrorMessage(error) });
      } finally {
        patchState(store, { isLoading: false });
      }
    },

    async loadProjectById(projectId: string): Promise<void> {
      patchState(store, { isLoading: true, error: null });
      try {
        const result = await firstValueFrom(projectApiService.getProjectById(projectId));
        patchState(store, { selectedProject: result });
      } catch (error) {
        patchState(store, { error: readErrorMessage(error) });
      } finally {
        patchState(store, { isLoading: false });
      }
    },

    async createProject(request: CreateProjectRequest): Promise<void> {
      patchState(store, { isLoading: true, error: null });
      try {
        await firstValueFrom(projectApiService.createProject(request));
      } catch (error) {
        patchState(store, { error: readErrorMessage(error) });
      } finally {
        patchState(store, { isLoading: false });
      }
    },

    async updateProject(
      projectId: string,
      request: UpdateProjectRequest,
    ): Promise<void> {
      patchState(store, { isLoading: true, error: null });
      try {
        await firstValueFrom(projectApiService.updateProject(projectId, request));
      } catch (error) {
        patchState(store, { error: readErrorMessage(error) });
      } finally {
        patchState(store, { isLoading: false });
      }
    },
  })),
);

function readErrorMessage(error: unknown): string {
  if (error instanceof Error && error.message.trim() !== '') {
    return error.message;
  }

  return 'Project operation failed.';
}
