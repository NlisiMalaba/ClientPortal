import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';

import {
  NotificationApiService,
  NotificationItem,
  NotificationPreference,
  NotificationQuery,
} from '../api/services/notification-api.service';

interface NotificationState {
  notifications: NotificationItem[];
  preferences: NotificationPreference[];
  unreadCount: number;
  totalCount: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: NotificationState = {
  notifications: [],
  preferences: [],
  unreadCount: 0,
  totalCount: 0,
  isLoading: false,
  error: null,
};

export const NotificationStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods(
    (store, notificationApiService = inject(NotificationApiService)) => ({
      async loadNotifications(query?: NotificationQuery): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const result = await firstValueFrom(
            notificationApiService.getNotifications(query),
          );
          patchState(store, {
            notifications: result.items,
            totalCount: result.totalCount,
            unreadCount: result.items.filter((item) => !item.isRead).length,
          });
        } catch (error) {
          patchState(store, { error: readErrorMessage(error) });
        } finally {
          patchState(store, { isLoading: false });
        }
      },

      async markAsRead(notificationId: string): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          await firstValueFrom(notificationApiService.markAsRead(notificationId));
          patchState(store, {
            notifications: store
              .notifications()
              .map((item) =>
                item.id === notificationId ? { ...item, isRead: true } : item,
              ),
            unreadCount: Math.max(store.unreadCount() - 1, 0),
          });
        } catch (error) {
          patchState(store, { error: readErrorMessage(error) });
        } finally {
          patchState(store, { isLoading: false });
        }
      },

      async loadPreferences(): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const preferences = await firstValueFrom(
            notificationApiService.getPreferences(),
          );
          patchState(store, { preferences });
        } catch (error) {
          patchState(store, { error: readErrorMessage(error) });
        } finally {
          patchState(store, { isLoading: false });
        }
      },

      async updatePreferences(
        preferences: NotificationPreference[],
      ): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          await firstValueFrom(notificationApiService.updatePreferences(preferences));
          patchState(store, { preferences });
        } catch (error) {
          patchState(store, { error: readErrorMessage(error) });
        } finally {
          patchState(store, { isLoading: false });
        }
      },
    }),
  ),
);

function readErrorMessage(error: unknown): string {
  if (error instanceof Error && error.message.trim() !== '') {
    return error.message;
  }

  return 'Notification operation failed.';
}
