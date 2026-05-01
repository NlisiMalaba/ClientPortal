import { Injectable, computed, signal } from '@angular/core';

export type ToastVariant = 'error' | 'success' | 'info' | 'warning';

export interface ToastNotification {
  id: string;
  message: string;
  variant: ToastVariant;
}

@Injectable({ providedIn: 'root' })
export class ToastNotificationService {
  private readonly notifications = signal<ToastNotification[]>([]);
  readonly activeNotifications = computed(() => this.notifications());

  show(message: string, variant: ToastVariant = 'info'): void {
    const notification: ToastNotification = {
      id: this.createId(),
      message,
      variant,
    };

    this.notifications.update((current) => [...current, notification]);
    window.setTimeout(() => this.dismiss(notification.id), 4500);
  }

  dismiss(id: string): void {
    this.notifications.update((current) =>
      current.filter((notification) => notification.id !== id),
    );
  }

  private createId(): string {
    return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
