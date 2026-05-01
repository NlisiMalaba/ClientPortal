import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  computed,
  input,
  output,
} from '@angular/core';

import { cn } from '@/components/lib/utils';

@Component({
  selector: 'ui-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (open()) {
      <div
        class="fixed inset-0 z-40 bg-background/70 backdrop-blur-sm"
        (click)="onBackdropClick()"
      ></div>

      <section
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        [attr.aria-modal]="true"
        [attr.aria-label]="ariaLabel()"
      >
        <div
          [class]="dialogClasses()"
          (click)="$event.stopPropagation()"
        >
          @if (showCloseButton()) {
            <button
              type="button"
              class="absolute right-4 top-4 rounded-md border border-input bg-background px-2 py-1 text-xs text-muted-foreground hover:bg-muted"
              aria-label="Close dialog"
              (click)="close()"
            >
              x
            </button>
          }
          <ng-content />
        </div>
      </section>
    }
  `,
})
export class DialogComponent {
  readonly open = input(false);
  readonly ariaLabel = input('Dialog');
  readonly showCloseButton = input(true);
  readonly closeOnBackdrop = input(true);
  readonly closeOnEscape = input(true);
  readonly class = input('');

  readonly openChange = output<boolean>();
  readonly closed = output<void>();

  readonly dialogClasses = computed(() =>
    cn(
      'relative w-full max-w-lg rounded-xl border bg-card text-card-foreground p-6 shadow-lg',
      this.class(),
    ),
  );

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.open() && this.closeOnEscape()) {
      this.close();
    }
  }

  onBackdropClick(): void {
    if (this.closeOnBackdrop()) {
      this.close();
    }
  }

  close(): void {
    this.openChange.emit(false);
    this.closed.emit();
  }
}

@Component({
  selector: 'ui-dialog-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: {
    '[class]': 'classes()',
  },
})
export class DialogHeaderComponent {
  readonly class = input('');
  readonly classes = computed(() =>
    cn('mb-4 flex flex-col gap-1.5 text-left', this.class()),
  );
}

@Component({
  selector: 'ui-dialog-title',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: {
    '[class]': 'classes()',
  },
})
export class DialogTitleComponent {
  readonly class = input('');
  readonly classes = computed(() => cn('text-lg font-semibold leading-none', this.class()));
}

@Component({
  selector: 'ui-dialog-description',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: {
    '[class]': 'classes()',
  },
})
export class DialogDescriptionComponent {
  readonly class = input('');
  readonly classes = computed(() => cn('text-sm text-muted-foreground', this.class()));
}

@Component({
  selector: 'ui-dialog-content',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: {
    '[class]': 'classes()',
  },
})
export class DialogContentComponent {
  readonly class = input('');
  readonly classes = computed(() => cn('py-1', this.class()));
}

@Component({
  selector: 'ui-dialog-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: {
    '[class]': 'classes()',
  },
})
export class DialogFooterComponent {
  readonly class = input('');
  readonly classes = computed(() =>
    cn('mt-6 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end', this.class()),
  );
}
