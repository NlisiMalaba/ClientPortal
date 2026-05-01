import { Injectable, computed, signal } from '@angular/core';

export type ToastVariant = 'error' | 'success' | 'info' | 'warning';

export interface ToastNotification {
  id: string;
  message: string;
  variant: ToastVariant;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class ToastNotificationService {
  private static readonly DEFAULT_DURATION_MS = 4500;

  private readonly notifications = signal<ToastNotification[]>([]);
  readonly activeNotifications = computed(() => this.notifications());

  show(
    message: string,
    variant: ToastVariant = 'info',
    durationMs = ToastNotificationService.DEFAULT_DURATION_MS,
  ): void {
    const notification: ToastNotification = {
      id: this.createId(),
      message,
      variant,
      durationMs,
    };

    this.notifications.update((current) => [...current, notification]);
    window.setTimeout(() => this.dismiss(notification.id), notification.durationMs);
  }

  success(message: string, durationMs?: number): void {
    this.show(message, 'success', durationMs);
  }

  error(message: string, durationMs?: number): void {
    this.show(message, 'error', durationMs);
  }

  warning(message: string, durationMs?: number): void {
    this.show(message, 'warning', durationMs);
  }

  info(message: string, durationMs?: number): void {
    this.show(message, 'info', durationMs);
  }

  dismiss(id: string): void {
    this.notifications.update((current) =>
      current.filter((notification) => notification.id !== id),
    );
  }

  clear(): void {
    this.notifications.set([]);
  }

  private createId(): string {
    return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
