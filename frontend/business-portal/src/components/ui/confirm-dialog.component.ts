import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { ButtonComponent } from './button.component';
import {
  DialogComponent,
  DialogContentComponent,
  DialogDescriptionComponent,
  DialogFooterComponent,
  DialogHeaderComponent,
  DialogTitleComponent,
} from './dialog.component';

@Component({
  selector: 'ui-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ButtonComponent,
    DialogComponent,
    DialogHeaderComponent,
    DialogTitleComponent,
    DialogDescriptionComponent,
    DialogContentComponent,
    DialogFooterComponent,
  ],
  template: `
    <ui-dialog
      [open]="open()"
      [ariaLabel]="ariaLabel()"
      [closeOnBackdrop]="!loading()"
      [closeOnEscape]="!loading()"
      (openChange)="openChange.emit($event)"
      (closed)="onCancelled()"
    >
      <ui-dialog-header>
        <ui-dialog-title>{{ title() }}</ui-dialog-title>
        <ui-dialog-description>{{ message() }}</ui-dialog-description>
      </ui-dialog-header>

      <ui-dialog-content>
        <ng-content />
      </ui-dialog-content>

      <ui-dialog-footer>
        <ui-button
          variant="outline"
          [disabled]="loading()"
          [label]="cancelLabel()"
          (clicked)="onCancelled()"
        />
        <ui-button
          variant="destructive"
          [disabled]="loading()"
          [label]="confirmButtonLabel()"
          (clicked)="onConfirmed()"
        />
      </ui-dialog-footer>
    </ui-dialog>
  `,
})
export class ConfirmDialogComponent {
  readonly open = input(false);
  readonly loading = input(false);
  readonly title = input('Confirm action');
  readonly message = input('This action cannot be undone.');
  readonly confirmLabel = input('Confirm');
  readonly cancelLabel = input('Cancel');
  readonly ariaLabel = input('Confirmation dialog');

  readonly openChange = output<boolean>();
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  confirmButtonLabel(): string {
    return this.loading() ? 'Processing...' : this.confirmLabel();
  }

  onConfirmed(): void {
    if (this.loading()) {
      return;
    }

    this.confirmed.emit();
  }

  onCancelled(): void {
    if (this.loading()) {
      return;
    }

    this.openChange.emit(false);
    this.cancelled.emit();
  }
}
