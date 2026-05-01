import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { ToastNotificationService, ToastVariant } from './toast-notification.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  template: `
    <section class="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      @for (notification of toastService.activeNotifications(); track notification.id) {
        <article
          class="rounded-md border px-4 py-3 shadow-md text-sm"
          [class]="resolveClass(notification.variant)"
          role="status"
          [attr.aria-live]="liveRegionPoliteness(notification.variant)"
        >
          <div class="flex items-start justify-between gap-3">
            <p>{{ notification.message }}</p>
            <button
              type="button"
              class="font-semibold opacity-70 hover:opacity-100"
              aria-label="Dismiss notification"
              (click)="toastService.dismiss(notification.id)"
            >
              x
            </button>
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastHostComponent {
  readonly toastService = inject(ToastNotificationService);

  resolveClass(variant: ToastVariant): string {
    switch (variant) {
      case 'error':
        return 'border-red-200 bg-red-50 text-red-700';
      case 'success':
        return 'border-green-200 bg-green-50 text-green-700';
      case 'warning':
        return 'border-amber-200 bg-amber-50 text-amber-700';
      default:
        return 'border-blue-200 bg-blue-50 text-blue-700';
    }
  }

  liveRegionPoliteness(variant: ToastVariant): 'assertive' | 'polite' {
    return variant === 'error' ? 'assertive' : 'polite';
  }
}
